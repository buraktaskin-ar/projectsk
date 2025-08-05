namespace ChatWithAPIDemo.Services
{
    public interface ISpeakerService
    {
        List<int?> GetBatteryLevels();
        void ChangeVolume(int id, int volume);
        string GetAllSpeakers();
        string MuteSpeaker(int speakerId);
        string GetMutedSpeakers();
        string GetUnmutedSpeakers();
        string GetSpeakerById(int id);
        string AddSpeaker(string name, int batteryLevel, int volumeLevel);
    }
}
