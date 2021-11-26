using Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace ScheduleVKBotApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        /// <summary>
        /// Конфигурация приложения
        /// </summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;
        private readonly CheckUpdate _currentSchedule;

        public CallbackController(IVkApi vkApi, IConfiguration configuration, CheckUpdate currentSchedule)
        {
            _vkApi = vkApi;
            _configuration = configuration;
            _currentSchedule = currentSchedule;
        }

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            // Проверяем, что находится в поле "type" 
            switch (updates.Type)
            {
                // Если это уведомление для подтверждения адреса
                case "confirmation":
                    // Отправляем строку для подтверждения 
                    return Ok(_configuration["Config:Confirmation"]);
                case "message_new":
                    {
                        // Десериализация
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        // Отправим в ответ cообщение
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            //RandomId = DateTime.Now.Millisecond,
                            RandomId = new Random().Next(),
                            PeerId = msg.PeerId.Value,
                            Message = MyAppLogic.ProcessingMessage(msg.Text.Trim(), msg.PeerId.Value, _currentSchedule) //Обработка и решение что отправлять в ответ клиенту
                        }) ;
                        break;
                    }
                case "test":
                    {
                        _currentSchedule.Test();
                        break;
                    }
            }
            // Возвращаем "ok" серверу Callback API
            return Ok("ok");
        }
        [HttpGet]
        public IActionResult Callback()
        {
            return Ok("ok");//для сервиса New Relic One
        }
    }
}
