using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDb_local_crawler {
    public class Utils {
        public static string Compress(FileInfo fileToCompress) {
            string result = null;
            using (FileStream originalFileStream = fileToCompress.OpenRead()) {
                if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz") {
                    using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz")) {
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress)) {
                            originalFileStream.CopyTo(compressionStream);
                            Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                                fileToCompress.Name, fileToCompress.Length.ToString(), compressedFileStream.Length.ToString());
                        }
                        result = compressedFileStream.Name;
                    }
                }
            }
            return result;
        }

        public static string Decompress(FileInfo fileToDecompress) {
            string result = null;
            using (FileStream originalFileStream = fileToDecompress.OpenRead()) {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName)) {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress)) {
                        decompressionStream.CopyTo(decompressedFileStream);
                        result = newFileName;
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
            return result;
        }
    }
}
