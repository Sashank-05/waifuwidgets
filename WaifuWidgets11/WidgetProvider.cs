// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WaifuWidgets11;
using Microsoft.Windows.Widgets.Providers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

[ComVisible(true)]
[ComDefaultInterface(typeof(IWidgetProvider))]
[Guid("B3B66BAB-E252-46BD-B354-E7CB88D04F1F")]
public sealed partial class WidgetProvider : IWidgetProvider
{
    public WidgetProvider()
    {
        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        AllocConsole();

        // Log that the provider has started
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", "WidgetProvider started\n");

        RecoverRunningWidgets();
    }

    private static bool HaveRecoveredWidgets { get; set; } = false;
    private static void RecoverRunningWidgets()
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", "Inside RecoverRunningWidgets\n");

        if (!HaveRecoveredWidgets)
        {
            try
            {
                var widgetManager = WidgetManager.GetDefault();
                foreach (var widgetInfo in widgetManager.GetWidgetInfos())
                {
                    var context = widgetInfo.WidgetContext;
                    if (!WidgetInstances.ContainsKey(context.Id))
                    {
                        if (WidgetImpls.ContainsKey(context.DefinitionId))
                        {
                            WidgetInstances[context.Id] = WidgetImpls[context.DefinitionId](context.Id, widgetInfo.CustomState);
                        }
                        else
                        {
                            widgetManager.DeleteWidget(context.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"Recover error: {ex.Message}\n");
            }
            finally
            {
                HaveRecoveredWidgets = true;
            }
        }
    }

    private static readonly Dictionary<string, WidgetCreateDelegate> WidgetImpls = new()
    {
        [WaifuWidget.DefinitionId] = (widgetId, initialState) => new WaifuWidget(widgetId, initialState)
    };

    private static Dictionary<string, WidgetImplBase> WidgetInstances = new();

    public void CreateWidget(WidgetContext widgetContext)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"CreateWidget id: {widgetContext.Id}\n");

        if (!WidgetImpls.ContainsKey(widgetContext.DefinitionId))
        {
            string errorMessage = $"ERROR: Requested unknown Widget Definition {widgetContext.DefinitionId}\n";
            File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", errorMessage);
            throw new Exception(errorMessage);
        }

        var widgetInstance = WidgetImpls[widgetContext.DefinitionId](widgetContext.Id, "");
        WidgetInstances[widgetContext.Id] = widgetInstance;

        WidgetUpdateRequestOptions options = new WidgetUpdateRequestOptions(widgetContext.Id);
        options.Template = widgetInstance.GetTemplateForWidget();
        options.Data = widgetInstance.GetDataForWidget();
        options.CustomState = widgetInstance.State;

        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", "Sending payload\n");

        try
        {
            WidgetManager.GetDefault().UpdateWidget(options);
        }
        catch (Exception ex)
        {
            File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"Widget update error: {ex.Message}\n");
        }
    }

    public void DeleteWidget(string widgetId, string _)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"DeleteWidget id: {widgetId}\n");

        WidgetInstances.Remove(widgetId);
    }

    public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"OnActionInvoked id: {actionInvokedArgs.WidgetContext.Id}\n");

        WidgetInstances[actionInvokedArgs.WidgetContext.Id].OnActionInvoked(actionInvokedArgs);
    }

    public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"OnWidgetContextChanged id: {contextChangedArgs.WidgetContext.Id}\n");

        WidgetInstances[contextChangedArgs.WidgetContext.Id].OnWidgetContextChanged(contextChangedArgs);
    }

    public void Activate(WidgetContext widgetContext)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"Activate id: {widgetContext.Id}\n");

        if (!WidgetInstances.ContainsKey(widgetContext.Id))
        {
            throw new Exception($"Activate called for unknown widget {widgetContext.Id}");
        }
    }

    public void Deactivate(string widgetId)
    {
        File.AppendAllText("C:\\Users\\sasha\\widget_log.txt", $"Deactivate id: {widgetId}\n");

        WidgetInstances[widgetId].Deactivate();
    }
}
