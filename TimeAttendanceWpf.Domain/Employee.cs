namespace TimeAttendanceWpf.Domain;

public enum EmployeeRole
{
    Employee,
    Manager
}

public sealed class Employee
{
    public Guid Id { get; }
    public string Name { get; }
    public EmployeeRole Role { get; }

    public Employee(Guid id, string name, EmployeeRole role)
    {
        Id = id;
        Name = name;
        Role = role;
    }
}
