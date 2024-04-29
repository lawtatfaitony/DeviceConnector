using Newtonsoft.Json;

namespace VxGuardClient.ModelView
{
    public class CropModel
    {
        [JsonProperty("clientPicUrl")]
        public string ClientPicUrl;

        [JsonProperty("ImageData")]
        public ImageData ImageData;

        [JsonProperty("CropBoxData")]
        public CropBoxData CropBoxData;
    }

    public class ImageData
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("rotate")]
        public int Rotate { get; set; }

        [JsonProperty("scaleX")]
        public int ScaleX { get; set; }

        [JsonProperty("scaleY")]
        public int ScaleY { get; set; }
    }

    public class CropBoxData
    {
        [JsonProperty("left")]
        public int Left { get; set; }

        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class CropBoxDataExtend
    {
        [JsonProperty("clientPicUrl")]
        public string ClientPicUrl { get; set; }

        [JsonProperty("left")]
        public float Left { get; set; }

        [JsonProperty("top")]
        public float Top { get; set; }

        [JsonProperty("width")]
        public float Width { get; set; }

        [JsonProperty("height")]
        public float Height { get; set; }
    }
}