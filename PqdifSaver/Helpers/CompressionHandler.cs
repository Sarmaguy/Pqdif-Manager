using System.IO.Compression;

/// <summary>
/// Provides static methods for compressing and decompressing waveform data using GZip.
/// Supports both float[] and object[] input for compression.
/// </summary>
public class CompresssionHandler
{
    /// <summary>
    /// Compresses an array of float values into a GZip-compressed byte array.
    /// </summary>
    /// <param name="values">The float array to compress.</param>
    /// <returns>Compressed byte array, or null if input is null or empty.</returns>
    public static byte[] CompressWaveform(float[] values)
    {
        if (values == null || values.Length == 0)
            return null;

 
        byte[] floatBytes = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, floatBytes, 0, floatBytes.Length);


        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                gzip.Write(floatBytes, 0, floatBytes.Length);
            }
            return output.ToArray();
        }
    }

    /// <summary>
    /// Compresses an array of objects (converted to float) into a GZip-compressed byte array.
    /// </summary>
    /// <param name="values">The object array to compress.</param>
    /// <returns>Compressed byte array, or null if input is null or empty.</returns>
    public static byte[] CompressWaveform(object[] values)
    {
        if (values == null || values.Length == 0)
            return null;


        float[] floatValues = Array.ConvertAll(values, item => Convert.ToSingle(item));

        return CompressWaveform(floatValues);
    }
    
    /// <summary>
    /// Decompresses a GZip-compressed byte array into a float array.
    /// </summary>
    /// <param name="compressed">The compressed byte array.</param>
    /// <returns>Decompressed float array, or null if input is null or empty.</returns>
    public static float[] DecompressWaveform(byte[] compressed)
    {
        if (compressed == null || compressed.Length == 0)
            return null;

        using (var input = new MemoryStream(compressed))
        using (var gzip = new GZipStream(input, CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            byte[] bytes = output.ToArray();
            
            float[] values = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
            return values;
        }
    }

}