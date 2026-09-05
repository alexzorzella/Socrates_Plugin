using UnityEngine;

public static class AlexCompression {
    const int compressTo = 42;

    public enum CompressionType {
        SMOOTH,
        QUICK
    }

    /// <summary>
    /// Returns the original vector2 scaled by the targetSize, anchored on the passed axis
    /// </summary>
    /// <param name="originalSize"></param>
    /// <param name="targetSize"></param>
    /// <param name="anchorWidth"></param>
    /// <returns></returns>
    public static Vector2 ProportionalSize(Vector2 originalSize, float targetSize, bool anchorWidth = true) {
        Vector2 result;

        if (anchorWidth) {
            result = new Vector2(targetSize, (originalSize.y / originalSize.x) * targetSize);
        } else {
            result = new Vector2((originalSize.x / originalSize.y) * targetSize, targetSize);
        }

        return result;
    }

    /// <summary>
    /// Returns a compressed version of the sprite with the passed name found in a /Resources/ folder.
    /// Smooth compression will average the color of neighboring pixels while quick compression will
    /// use the color of the pixels from a grid.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="compressionMode"></param>
    /// <returns></returns>
    public static Sprite CompressSprite(string name, CompressionType compressionMode = CompressionType.SMOOTH) {
        Sprite toCompress = ResourceLoader.LoadSprite(name);

        if (toCompress == null) {
            return null;
        }

        float actualWidth = toCompress.texture.width;
        float actualHeight = toCompress.texture.height;

        bool horizontal = actualWidth > actualHeight;
        int targetWidth = compressTo;
        int targetHeight = compressTo;

        // actualWidth     targetWidth
        // -----------  =  ------------
        // actualHeight    targetHeight
            
        if (horizontal) {
            targetWidth = (int)(compressTo * (actualWidth / actualHeight));
        }
        else {
            targetHeight = (int)(compressTo * (actualHeight / actualWidth));
        }

        Texture2D texture = new Texture2D(targetWidth, targetHeight);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                texture.SetPixel(x, y, new Color(1, 1, 1, 0));
            }
        }

        int incrementX = (int)(actualWidth / targetWidth);
        int incrementY = (int)(actualHeight / targetHeight);

        for (int x = 0; x < targetWidth; x++) {
            for (int y = 0; y < targetHeight; y++) {
                Color finalColor = new Color();

                if (compressionMode == CompressionType.SMOOTH) {
                    float finalR = 0;
                    float finalG = 0;
                    float finalB = 0;

                    for (int w = 0; w < incrementX; w++) {
                        for (int h = 0; h < incrementY; h++) {
                            Color pixel = toCompress.texture.GetPixel(x * incrementX + w, y * incrementY + h);

                            finalR += pixel.r;
                            finalG += pixel.g;
                            finalB += pixel.b;
                        }
                    }

                    int area = incrementX * incrementY;
                    finalColor = new Color(finalR / area, finalG / area, finalB / area, 1);
                }
                else {
                    finalColor = toCompress.texture.GetPixel(x * incrementX, y * incrementY);
                }

                texture.SetPixel(x, y, finalColor);
            }
        }

        texture.Apply();

        Sprite result = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F),
            compressTo);
        result.name = $"{name}_{compressionMode.ToString().ToLower()}ly_compressed_{targetWidth}x{targetHeight}";
        return result;
    }
}