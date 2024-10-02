namespace DropletsInMotion.Infrastructure.Services;

public class UserService : IUserService
{
    private IUserService.CommunicationType _communication = IUserService.CommunicationType.NotSet;

    public event EventHandler? CommunicationTypeChanged;

    public required string PlatformPath { get; set; }
    public required string ProgramPath { get; set; }

    public IUserService.CommunicationType Communication
    {
        get => _communication;
        set
        {
            if (_communication != value)
            {
                _communication = value;
                OnCommunicationTypeChanged();
            }
        }
    }

    public virtual void OnCommunicationTypeChanged()
    {
        CommunicationTypeChanged?.Invoke(this, EventArgs.Empty);
    }
}