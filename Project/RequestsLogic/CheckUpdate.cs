using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using VkNet.Abstractions;
using VkNet.Model.RequestParams;
using Logic.DataLayer;
using VkNet.Model;

namespace Logic
{
    public class CheckUpdate
    {
        public IVkApi _vkApi { get; set; }
        public List<string> current { get; set; }
        public string dateOfCurrent { get; set; }
        public CheckUpdate(IVkApi api)
        {
            _vkApi = api;
        }
        public void Check(string first = "h", string second = "g", string number = "")
        {
            var result = Parse("li");

            if (dateOfCurrent == null && result != null)
            {
                dateOfCurrent = result[3];
                current = Parse();
                _vkApi.Messages.Send(new MessagesSendParams //лог для себя
                {
                    RandomId = new Random().Next(),
                    PeerId = 341355701,
                    Message = "Я начал работать"
                });
            }
            else if (result[3] != dateOfCurrent)
            {
                dateOfCurrent = result?[3];
                current = Parse();
                //уведомить о изменении
                //реализация уведомления меня и друзей
                List<StudentGroup> students = new List<StudentGroup>();
                students.Add(new StudentGroup(193906143, "0219-ЭК(о)"));//me
                students.Add(new StudentGroup(315165127, "0219-ЭК(о)"));//ang
                students.Add(new StudentGroup(341355701, "0219-ЭК(о)"));//sasha
                foreach (var student in students)
                {
                    _vkApi.Messages.Send(new MessagesSendParams
                    {
                        RandomId = new Random().Next(),
                        PeerId = student.peer_id,
                        Message = MessageOfUpdate(student.group)
                    });
                }
            }
        }
        // selector: выборка html элементов (базово <tr>...</tr>)
        // first: h - ежедневное, с - еженедельное, b - основное
        // second: g - по группам, p - по преподам, a - по аудиториям
        // number: номер группы||препода||аудитории
        public List<string> Parse(string selector = "tr", string first = "h", string second = "g", string number = "")
        {
            var request = new GetRequest($"http://dmitrov-dubna.ru/shedule/{first}{second}{number}.htm");
            request.Run();
            var list = new List<string>();
            var domParser = new HtmlParser();
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> items;
            try
            {
                var document = domParser.ParseDocument(request.Response);
                items = document.QuerySelectorAll(selector);
            }
            catch 
            { 
                return null;
            }
            foreach (var item in items)
            {
                list.Add(item.TextContent);
            }
            return list;
        }

        public void RunPeriodicialy()
        {
            Task timerTask = Checking(TimeSpan.FromMinutes(5));//запуск обновления каждые N минут
        }

        public async Task Checking(TimeSpan interval)
        {
            while (true)
            {
                Check();
                _vkApi.Messages.Send(new MessagesSendParams //лог отправляемый мне в лс для удаленного понимания работы
                {
                    RandomId = new Random().Next(),
                    PeerId = 341355701,
                    Message = dateOfCurrent // + " тест"
                });

                await Task.Delay(interval);
            }
        }
        public string MessageOfUpdate(string value)
        {
            string result = $"‼Расписание изменилось! Вот расписание на {dateOfCurrent}:\n\n";
            bool find = false;
            int num = 1;
            for (int i = 0; i < current.Capacity; i++)
            {
                if (find && num < 6)
                {
                    if (num == 2)
                        result += "2⃣";
                    else if (num == 3)
                        result += "3⃣";
                    else if (num == 4)
                        result += "4⃣";
                    else if (num == 5)
                        result += "5⃣";
                    string str = current[i].Substring(1);
                    for (int j = 0; j < 18; j++)
                        if (str[j] == ':')
                            str = str.Insert(j + 1, " ");
                    result += $"{str.Substring(0, 18)}\n";
                    if (str.Length > 19)
                    {
                        for (int j = 19; j < str.Length; j++)
                            if (str[j] == ')')
                                str = str.Insert(j + 1, "\n");
                        result += $"{str.Substring(18)}\n\n";
                    }
                    else result += "\n";
                    num++;
                    if (num == 6)
                        return result;
                }
                else if (current[i].StartsWith(value))
                {
                    find = true;
                    string str = current[i].Substring(value.Length + 1);
                    for (int j = 0; j < 18; j++)
                        if (str[j] == ':')
                            str = str.Insert(j + 1, " ");
                    result += $"1⃣{str.Substring(0, 18)}\n";
                    if (str.Length > 19)
                    {
                        for (int j = 19; j < str.Length; j++)
                            if (str[j] == ')')
                                str = str.Insert(j + 1, "\n");
                        result += $"{str.Substring(18)}\n\n";
                    }
                    else result += "\n";
                    num++;
                }

            }
            return result;
        }
        public void Test()//функция для тестирования без выгрузки на сервер
        {
            List<StudentGroup> students = new List<StudentGroup>();
            students.Add(new StudentGroup(193906143, "0219-ЭК(о)"));//me
            students.Add(new StudentGroup(315165127, "0219-ЭК(о)"));//ang
            students.Add(new StudentGroup(341355701, "0219-ЭК(о)"));//sasha
            foreach (var student in students)
            {
                _vkApi.Messages.Send(new MessagesSendParams
                {
                    RandomId = new Random().Next(),
                    PeerId = student.peer_id,
                    Message = MessageOfUpdate(student.group)
                });
            }
        }
    }
}
