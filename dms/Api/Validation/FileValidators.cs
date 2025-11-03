using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace dms.Api.Validation;

public static class FileValidators
{
    public static async Task<bool> IsPdfAsync(Stream stream, CancellationToken ct = default)
    {
        if (!stream.CanSeek) return false;

        long pos = stream.Position;
        try
        {
            stream.Position = 0;
            byte[] header = new byte[5];
            int read = await stream.ReadAsync(header, 0, header.Length, ct);
            return read == 5 &&
                   header[0] == (byte)'%' &&
                   header[1] == (byte)'P' &&
                   header[2] == (byte)'D' &&
                   header[3] == (byte)'F' &&
                   header[4] == (byte)'-';
        }
        finally
        {
            if (stream.CanSeek) stream.Position = pos;
        }
    }
}
