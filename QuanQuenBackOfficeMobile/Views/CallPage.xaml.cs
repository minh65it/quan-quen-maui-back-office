public partial class CallPage : ContentPage
{
    readonly IAgoraService _agora;

    bool isMute = false;
    bool isSpeaker = true;

    public CallPage(IAgoraService agora)
    {
        InitializeComponent();
        _agora = agora;

        _agora.Initialize("YOUR_APP_ID");
    }

    async void Join_Clicked(object sender, EventArgs e)
    {
        await Permissions.RequestAsync<Permissions.Microphone>();

        _agora.JoinChannel("TOKEN", "testChannel", 0);
    }

    void Mute_Clicked(object sender, EventArgs e)
    {
        isMute = !isMute;
        _agora.Mute(isMute);
    }

    void Speaker_Clicked(object sender, EventArgs e)
    {
        isSpeaker = !isSpeaker;
        _agora.SetSpeaker(isSpeaker);
    }

    void Leave_Clicked(object sender, EventArgs e)
    {
        _agora.LeaveChannel();
    }
}