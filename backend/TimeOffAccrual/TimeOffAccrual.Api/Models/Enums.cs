namespace TimeOffAccrual.Api.Models;

public enum Role
{
    Admin,
    Agent
}

public enum RequestStatus
{
    Pending,
    Approved,
    Denied
}

public enum DurationType
{
    Full,
    Partial
}