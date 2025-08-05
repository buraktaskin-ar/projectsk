
using ChatWithAPIDemo.Plugins;

namespace ChatWithAPIDemo.Services
{
    public class SpeakerService : ISpeakerService
    {
        private readonly SpeakerPlugin _plugin;

        public SpeakerService(SpeakerPlugin plugin)
        {
            _plugin = plugin;
        }

        public List<int?> GetBatteryLevels() =>
            _plugin.GetBatteryLevels();

        public void ChangeVolume(int id, int volume) =>
            _plugin.ChangeVolume(id, volume);

        public string GetAllSpeakers() =>
            _plugin.GetVolumeLevels();

        public string MuteSpeaker(int speakerId) =>
            _plugin.MuteSpeaker(speakerId);

        public string GetMutedSpeakers() =>
            _plugin.GetMutedSpeakers();

        public string GetUnmutedSpeakers() =>
            _plugin.GetUnmutedSpeakers();

        public string GetSpeakerById(int id) =>
            _plugin.GetSpeakerById(id);

        public string AddSpeaker(string name, int batteryLevel, int volumeLevel) =>
            _plugin.AddSpeaker(name, batteryLevel, volumeLevel);
    }
}
