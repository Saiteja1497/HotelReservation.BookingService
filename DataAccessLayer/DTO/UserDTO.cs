namespace DataAccessLayer.DTO
{
    public record UserDTO(Guid UserID,string? Email, string? UserName, string Gender)
    {
        public UserDTO() : this(Guid.Empty,default,default,default) { }
    }
}
