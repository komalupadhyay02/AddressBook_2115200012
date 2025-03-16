using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;


namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;
        private readonly IRabbitMqProducer _rabbitMqProducer;

        //constructor of class
        public UserBL(IUserRL userRL,IRabbitMqProducer rabbitMqProducer)
        {
            _userRL = userRL;
            _rabbitMqProducer = rabbitMqProducer;
        }

        /// <summary>
        /// method to register the user
        /// </summary>
        /// <param name="userRegisterDTO">User Details to register</param>
        /// <returns>Returns user details if save else null</returns>
       
        public User RegisterUser(RegisterDTO userRegisterDTO)
        {
            var user= _userRL.RegisterUser(userRegisterDTO);
            if (user != null)
            {
                var userEvent = new UserEventDTO
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EventType = "UserRegistered"
                };
                _rabbitMqProducer.PublishMessage(userEvent);
            }
            return user;
        }
        /// <summary>
        /// method to login the user
        /// </summary>
        /// <param name="loginDTO">login credentials</param>
        /// <returns>Success or failure response</returns>
        public UserResponseDTO LoginUser(LoginDTO loginDTO)
        {
            return _userRL.LoginUser(loginDTO);
        }
        /// <summary>
        /// method to get the token on mail for forget password
        /// </summary>
        /// <param name="email">email of user </param>
        /// <returns>true or false id email exist or not</returns>
        public bool ForgetPassword(string email)
        {
            return _userRL.ForgetPassword(email);
        }
        /// <summary>
        /// method to reset the password 
        /// </summary>
        /// <param name="token">reset token from mail</param>
        /// <param name="newPassword">new password from user</param>
        /// <returns>true or false</returns>
        public bool ResetPassword(string token, string newPassword)
        {
            return _userRL.ResetPassword(token, newPassword);
        }
    }
}
