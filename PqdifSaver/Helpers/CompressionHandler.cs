using System.IO.Compression;

public class CompresssionHandler
{
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

    public static byte[] CompressWaveform(object[] values)
    {
        if (values == null || values.Length == 0)
            return null;


        float[] floatValues = Array.ConvertAll(values, item => Convert.ToSingle(item));

        return CompressWaveform(floatValues);
    }
    
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