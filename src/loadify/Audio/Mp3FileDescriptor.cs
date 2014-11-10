using System;
using System.IO;
using System.Linq;
using Id3;
using Id3.Frames;

namespace loadify.Audio
{
    public class Mp3FileDescriptor : IAudioFileDescriptor
    {
        public void Write(Mp3MetaData mp3MetaData, string inputFilePath)
        {
            using (var fileStream = new FileStream(inputFilePath, FileMode.Open))
            {
                using (var mp3 = new Mp3Stream(fileStream, Mp3Permissions.ReadWrite))
                {
                    mp3.DeleteAllTags(); // make sure the file got no tags

                    var id3Tag = new Id3Tag();
                    id3Tag.Title.Value = mp3MetaData.Title;
                    foreach(var artist in mp3MetaData.Artists)
                        id3Tag.Artists.Value.Add(artist);
                    id3Tag.Album.Value = mp3MetaData.Album;
                    id3Tag.Year.Value = mp3MetaData.Year;
                    id3Tag.Pictures.Add(new PictureFrame() { PictureType = PictureType.FrontCover, PictureData = mp3MetaData.Cover });

                    mp3.WriteTag(id3Tag, 2, 3);
                }
            }
        }

        public Mp3MetaData Read(string inputFilePath)
        {
            throw new NotImplementedException();
        }
    }
}
