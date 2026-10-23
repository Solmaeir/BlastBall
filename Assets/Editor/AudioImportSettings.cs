using UnityEditor;
using UnityEngine;

// Resources/Audio altına yeni atılan seslere uygun içe aktarma ayarını otomatik verir (yalnızca ilk içe aktarmada;
// sonradan elle değiştirilen ayarlar korunur).
//   SFX/   : kısa sesler, mono, açılışta çözülür (ADPCM) -> çalarken gecikme ve işlemci yükü olmaz
//   Music/ : uzun müzikler, akış (Streaming) + Vorbis -> bellekte yer tutmaz
public class AudioImportSettings : AssetPostprocessor
{
    private void OnPreprocessAudio()
    {
        if (!assetImporter.importSettingsMissing) return;

        var importer = (AudioImporter)assetImporter;
        ApplyFor(importer, assetPath);
    }

    public static void ApplyFor(AudioImporter importer, string path)
    {
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;

        if (path.Contains("/Resources/Audio/SFX/"))
        {
            importer.forceToMono = true;
            settings.preloadAudioData = true;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.ADPCM;
        }
        else if (path.Contains("/Resources/Audio/Music/"))
        {
            importer.forceToMono = false;
            settings.preloadAudioData = false;
            settings.loadType = AudioClipLoadType.Streaming;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.7f;
        }
        else
        {
            return;
        }

        importer.defaultSampleSettings = settings;
    }
}
