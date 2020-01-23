using PersonalPhotos.Controllers;
using Xunit;
using Moq;
using Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PersonalPhotos.Models;
using System.Threading.Tasks;
using Core.Models;

namespace PersonalPhotos.Test
{
    public class LoginsTests
    {
        private readonly LoginsController _controller;
        private readonly Mock<ILogins> _logins;
        private readonly Mock<IHttpContextAccessor> _accessor;

        public LoginsTests()
        {
            _logins = new Mock<ILogins>();
            

            var session = Mock.Of<ISession>();
            var httpContext = Mock.Of<HttpContext>(x => x.Session == session);

            _accessor = new Mock<IHttpContextAccessor>();
            _accessor.Setup(x => x.HttpContext).Returns(httpContext);

            _controller = new LoginsController(_logins.Object, _accessor.Object);
        }

        [Fact]
        public void Index_GivenNorReturnUrl_ReturnLoginView()
        {
            var result = (_controller.Index() as ViewResult);

            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName, ignoreCase: true);
        }

        [Fact]
        public async Task Login_GivenModelStateInvalid_ReturnLoginView()
        {
            _controller.ModelState.AddModelError("Test", "Test");//这里直接认定有错误，

            var result = await _controller.Login(Mock.Of<LoginViewModel>()) as ViewResult;//Mock.Of<LoginViewModel>()会帮我们创建一个LoginViewModel的实例，类似new。as对引用类型进行转换，无法转换时返回null而非抛出异常
            Assert.Equal("Login", result.ViewName, ignoreCase: true);
        }

        [Fact]
        public async Task Login_GivenCorrectPassword_RedirectToDisplayAction()
        {
            const string password = "123";
            var modelView = Mock.Of<LoginViewModel>(x=> x.Email == "a@b.com" && x.Password== password);//模拟LoginViewModel，这里的lamda expression看着像比较符，但是在mock里是赋值（assign）的作用
            var model = Mock.Of<User>(x=> x.Password == password);//同样，模拟User，用于GetUser方法的返回值。

            _logins.Setup(x => x.GetUser(It.IsAny<string>())).ReturnsAsync(model);//Setup方法非常有用，这里直接指定_login这个服务（生产环境中访问数据库）的Getuser方法的返回值等于model
            //It.IsAny<string>()表示：匹配给出的任意类型，说人话就是传入string类型，就随机产生字符串，int类型，就随机产生int数字
            var result = await _controller.Login(modelView);

            Assert.IsType<RedirectToActionResult>(result);
        }

        //现在测试 密码错误的情况
        [Fact]
        public async Task Login_InvalidPassword_ReturnToLoginView()
        {
            _controller.ModelState.AddModelError("password","密码错误");//只是为了表示错误
            var model = Mock.Of<LoginViewModel>();
          var result = await _controller.Login(model) as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName);
        }
    }
}
