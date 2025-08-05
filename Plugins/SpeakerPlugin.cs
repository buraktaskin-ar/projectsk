using ChatWithAPIDemo.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace ChatWithAPIDemo.Plugins
{
    public class SpeakerPlugin
    {



        List<SpeakerModel> speakers;

        public SpeakerPlugin(List<SpeakerModel> speakers)
        {
            this.speakers = speakers;
        }






        [KernelFunction("get_battery_levels")]
        [Description("Returns the battery levels of all speaker")]
        public List<int?> GetBatteryLevels()
        {
            List<int?> batteryLevels = new List<int?>();
            foreach (var speaker in speakers)
            {
                batteryLevels.Add(speaker.BatteryLevel);
            }
            return batteryLevels;
        }




        [KernelFunction("set_volume_speaker")]
        [Description("Sets the volume of a speaker")]
        public void ChangeVolume(int id, int volume)
        {
            var speaker = speakers.FirstOrDefault(s => s.Id == id);
            if (speaker != null)
            {
                speaker.VolumeLevel = volume;
            }
        }




        [KernelFunction("get_all_speakers"),
         Description("Returns an array of objects {id,name,volume_level} for every speaker.")]
        public string GetVolumeLevels()
        {
            var items = speakers.Select(s => new
            {
                s.Id,
                s.Name,
                volume_level = s.VolumeLevel
            });
            return JsonSerializer.Serialize(items);
        }


        [KernelFunction("mute_speaker"),
        Description("Mutes the specified speaker (volume stays in memory). Returns updated speaker.")]
        public string MuteSpeaker([Description("Id of the speaker")] int speakerId)
        {
            var spk = speakers.FirstOrDefault(s => s.Id == speakerId)
                      ?? throw new ArgumentException($"Speaker {speakerId} not found");
            spk.IsMuted = true;
            return JsonSerializer.Serialize(spk);
        }



        [KernelFunction("get_muted_speakers")]
        [Description("Returns an array of speaker objects that are currently muted.")]
        public string GetMutedSpeakers()
        {
            var muted = speakers
                .Where(s => s.IsMuted == true)
                .Select(s => new { s.Id, s.Name, s.VolumeLevel, s.IsMuted });
            return JsonSerializer.Serialize(muted);
        }




        [KernelFunction("get_unmuted_speakers")]
        [Description("Returns an array of speaker objects that are not muted.")]
        public string GetUnmutedSpeakers()
        {
            var unmuted = speakers
                .Where(s => s.IsMuted == false)
                .Select(s => new { s.Id, s.Name, s.VolumeLevel, s.IsMuted });
            return JsonSerializer.Serialize(unmuted);
        }



        [KernelFunction("get_speaker_by_id")]
        [Description("Returns the speaker object for the given Id.")]
        public string GetSpeakerById([Description("Id of the speaker")] int id)
        {
            var spk = speakers.FirstOrDefault(s => s.Id == id)
                      ?? throw new ArgumentException($"Speaker {id} not found");
            return JsonSerializer.Serialize(spk);
        }



      

        [KernelFunction("add_speaker")]
        [Description("Adds a new speaker with the given name, battery level and volume level, returns the created speaker.")]
        public string AddSpeaker(
            [Description("Name of the new speaker")] string name,
            [Description("Initial battery level")] int batteryLevel,
            [Description("Initial volume level")] int volumeLevel)
        {
            // Determine next Id
            var nextId = speakers.Any()
                ? speakers.Max(s => s.Id) + 1
                : 1;

            // Create and initialize
            var newSpeaker = new SpeakerModel
            {
                Id = nextId,
                Name = name,
                BatteryLevel = batteryLevel,
                VolumeLevel = volumeLevel,
                IsMuted = false
            };

            // Add to list
            speakers.Add(newSpeaker);

            // Return the new speaker as JSON
            return JsonSerializer.Serialize(newSpeaker);
        }













    }
}
