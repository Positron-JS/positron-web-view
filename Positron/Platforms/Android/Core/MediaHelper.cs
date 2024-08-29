using Android.Content;
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.Positron.Platforms;

    internal class MediaHelper
    {

        public static MediaHelper Instance { get; } = new MediaHelper();

        public async Task<List<string>> SaveImageToFile(Intent intent)
        {
            if (intent.Data != null)
            {
                return new List<string> { await SaveImageToFile(intent.Data) };
            }
            var tasks = new List<Task<string>>();
            var clipData = intent.ClipData;
            if (clipData != null)
            {
                for (int i = 0; i < clipData.ItemCount; i++)
                {
                    var data = clipData.GetItemAt(i);
                    if (data?.Uri != null)
                    {
                        tasks.Add(SaveImageToFile(data.Uri));
                    }
                }
            }
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<string> SaveImageToFile(global::Android.Net.Uri uri)
        {
            var source = ImageDecoder.CreateSource(
                Platform.CurrentActivity.ContentResolver!,
                uri!
            );
            var bitmap = ImageDecoder.DecodeBitmap(source);
            var f = Java.IO.File.CreateTempFile("img", ".png");
            using var s = System.IO.File.OpenWrite(f.AbsolutePath);
            await bitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, s);
            return f.AbsolutePath;
        }

    }

