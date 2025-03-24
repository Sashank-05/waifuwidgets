using Microsoft.Windows.Widgets.Providers;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WaifuWidgets11
{
    internal class WaifuWidget : WidgetImplBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public static string DefinitionId { get; } = "CSwaifu_widget";
        private string imageUrl;
        private string preloadedImageUrl;

        public WaifuWidget(string widgetId, string startingState) : base(widgetId, startingState)
        {
            if (string.IsNullOrEmpty(state))
            {
                imageUrl = "https://i.waifu.pics/rF-pZ8a.jpg";
            }
            else
            {
                var parsedState = JsonDocument.Parse(state);
                imageUrl = parsedState.RootElement.GetProperty("imageUrl").GetString();
            }

            Console.WriteLine($"🆕 WaifuWidget initialized: {widgetId} (Image: {imageUrl})");

            _ = PreloadNextImage(); // Preload the next image
        }

        public override async void Activate(WidgetContext widgetContext)
        {
            isActivated = true;
            Console.WriteLine($"✅ Widget {Id} activated.");

            // Switch to preloaded image if available
            if (!string.IsNullOrEmpty(preloadedImageUrl))
            {
                Console.WriteLine($"🌟 Using preloaded image: {preloadedImageUrl}");
                imageUrl = preloadedImageUrl;
                preloadedImageUrl = null;
                await UpdateWidget(); // Force refresh
            }

            _ = PreloadNextImage(); // Start preloading again
        }

        public override async void Deactivate()
        {
            isActivated = false;
            Console.WriteLine($"⛔ Widget {Id} deactivated. Preloading next image...");
            await PreloadNextImage(); // Always keep one image ready
        }

        public override async void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            if (actionInvokedArgs.Verb == "refreshButton")
            {
                Console.WriteLine("🔄 Refresh button clicked! Fetching new image...");
                await UpdateWidgetImage();
            }
        }

        public override string GetTemplateForWidget()
        {
            return @"
            {
                ""type"": ""AdaptiveCard"",
                ""body"": [
                    {
                        ""type"": ""Image"",
                        ""url"": ""${imageUrl}"",
                        ""size"": ""auto"",
                        ""altText"": ""Random Waifu Image""
                    }
                ],
                ""actions"": [
                    {
                        ""type"": ""Action.Submit"",
                        ""title"": ""Refresh"",
                        ""id"": ""refreshButton"",
                        ""data"": { ""action"": ""refresh"" }
                    }
                ],
                ""$schema"": ""http://adaptivecards.io/schemas/adaptive-card.json"",
                ""version"": ""1.6""
            }";
        }

        public override string GetDataForWidget()
        {
            var stateNode = new JsonObject
            {
                ["imageUrl"] = imageUrl
            };
            return stateNode.ToJsonString();
        }

        private async Task UpdateWidget()
        {
            Console.WriteLine($"🔄 Updating Widget {Id} with image: {imageUrl}");

            var updateOptions = new WidgetUpdateRequestOptions(Id)
            {
                Data = GetDataForWidget(),
                CustomState = GetDataForWidget()
            };

            WidgetManager.GetDefault().UpdateWidget(updateOptions);
            Console.WriteLine($"✅ Widget {Id} updated.");
        }

        private async Task UpdateWidgetImage()
        {
            string newImageUrl = await FetchWaifuImageUrl();
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                imageUrl = newImageUrl;
                Console.WriteLine($"📸 Updated image: {imageUrl}");
                await UpdateWidget();
                _ = PreloadNextImage(); // Preload another image after refresh
            }
        }

        private async Task PreloadNextImage()
        {
            string newImageUrl = await FetchWaifuImageUrl();
            if (!string.IsNullOrEmpty(newImageUrl))
            {
                preloadedImageUrl = newImageUrl;
                Console.WriteLine($"🔄 Preloaded image: {preloadedImageUrl}");
            }
        }

        private static async Task<string> FetchWaifuImageUrl()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync("https://api.waifu.pics/sfw/waifu");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("url", out JsonElement urlElement))
                {
                    return urlElement.GetString();
                }
            }
            catch (Exception ex)
            if (parsedState.RootElement.TryGetProperty("imageUrl", out JsonElement imageUrlElement))
            {
                imageUrl = imageUrlElement.GetString() ?? "https://i.waifu.pics/rF-pZ8a.jpg";
            }
            else
            {
                imageUrl = "https://i.waifu.pics/rF-pZ8a.jpg";
            }
            {
                Console.WriteLine($"❌ Error fetching image: {ex.Message}");
            }
            return string.Empty;
        }
    }
}
