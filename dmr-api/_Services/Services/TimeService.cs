using DMR_API._Services.Interface;
using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Services
{
    public class Response
    {
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public double Consumption { get; set; }
        public string Message { get; set; }
    }
    public class TimeServiceParams
    {
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public DateTime EndLunchTime { get; set; }
        public DateTime StartLunchTime { get; set; }

        public double PrepareTime { get; set; }

        public double Consumption { get; set; }
        public double HourlyOutput { get; set; }
        public double ReplacementFrequency { get; set; }
    }
    public class TimeService : ITimeService
    {
        public List<TodolistDto> GenerateTaskByTimeRange(TimeServiceParams model)
        {
            //var lunchHour = 1;
            //var ct = DateTime.Now;
            //var startLunchTime = model.StartLunchTime;
            //var finishLunchTime = model.EndLunchTime;
            //var finishWorkingTime = model.End;
            //var prepareTime = TimeSpan.FromMinutes(30);
            //double replacementFrequency = model.ReplacementFrequency;

            //var startWorkingTimeTemp = model.Start;
            //var fwt = new DateTime();
            //var kgPair = model.Consumption / 1000;
            //var hourlyOutput = model.HourlyOutput;
            //var end = model.End;
            //var list = new List<ToDoListDto>();
            //while (true)
            //{
            //    fwt = startWorkingTimeTemp.Add(prepareTime);
            //    var todo = new ToDoListDto();
            //    todo.GlueName = glue.Key.Name;
            //    todo.GlueID = item.GlueID;
            //    todo.PlanID = item.PlanID;
            //    todo.LineID = item.Building.ID;
            //    todo.LineName = item.Building.Name;
            //    todo.PlanID = item.PlanID;
            //    todo.BPFCID = item.BPFCID;
            //    todo.Supplier = item.ChemicalA.Supplier.Name;
            //    todo.PlanID = item.PlanID;
            //    todo.GlueNameID = item.GlueName.ID;
            //    todo.BuildingID = building.ID;

            //    if (startWorkingTimeTemp > end) break;
            //    //  12:30 >= SLT 12:30 and 13:30 <= FLT 13:30
            //    // TGBD ma lon hon TGBD An trua thi lay tu TGKT an trua tro di
            //    if (startWorkingTimeTemp >= startLunchTime && startWorkingTimeTemp <= finishLunchTime && fwt <= finishLunchTime || startWorkingTimeTemp > startLunchTime && startWorkingTimeTemp < finishLunchTime && fwt >= finishLunchTime)
            //    {
            //        startWorkingTimeTemp = finishLunchTime;
            //        todo.Start = startWorkingTimeTemp;  // SLT 13:30
            //        todo.End = startWorkingTimeTemp.Add(prepareTime); // 13:30 + preparetime
            //        todo.Message += $"Giao với giờ ăn trưa: {replacementFrequency} ";
            //        // 13:30 + 2 = 15:30 >= 14:00
            //        var finishWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
            //        if (finishWorkingTimeTemp >= finishWorkingTime)
            //        {
            //            replacementFrequency = (end - startWorkingTimeTemp).TotalHours;
            //            todo.Message += $"Vượt quá endTime: {replacementFrequency}";
            //        }
            //        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
            //        todo.Consumption = standardConsumption;

            //    }
            //    else
            //    {
            //        //Neu TGBD < TGBD an trua && TGKT nam trong khoang TG An Trua va replacementFrequency > khoangTGAn trua thi tinh lai consumption
            //        // 16:50 >= 16:30 -> 10minutes,
            //        replacementFrequency = fwt >= finishWorkingTime ? (finishWorkingTime - startWorkingTimeTemp).TotalHours : replacementFrequency;

            //        // TGKT > TGKT Hanh Chinh thì tính lại consumption
            //        if (fwt >= finishWorkingTime)
            //        {
            //            todo.Message += $"Thoi gian lam viec con lai co {replacementFrequency} gio. Tinh lai consumption";
            //        }
            //        todo.Start = startWorkingTimeTemp;
            //        var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
            //        todo.Consumption = standardConsumption;
            //        todo.End = fwt;

            //        // Nếu Cộng thêm replacementFrequency mà TG giao nhau voi TG an trua thi phai tru ra TG an trua
            //        var finishWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
            //        // EX: StartTime của nhiệm vụ tiếp theo là 13:30 , thì khoảng từ 11:30 -> 13:30 sẽ bị giao với TG ăn trua
            //        // 12:30 >= 11:30 and 9:30 >= 13:30
            //        if (startLunchTime >= startWorkingTimeTemp && finishWorkingTimeTemp >= finishLunchTime)
            //        {
            //            var recalculateReplacementFrequency = replacementFrequency - lunchHour;
            //            todo.Message += $"{startLunchTime.ToString("HH:mm")} >= {startWorkingTimeTemp.ToString("HH:mm")} && {finishWorkingTimeTemp.ToString("HH:mm")} >= {finishLunchTime.ToString("HH:mm")}";

            //            // Nếu FWTT 14:20 > FWT 14:00-> dư ra 20 phút thì phải trừ ra 20 phút
            //            if (finishWorkingTimeTemp >= finishWorkingTime)
            //            {
            //                var old = recalculateReplacementFrequency;
            //                recalculateReplacementFrequency = recalculateReplacementFrequency - (finishWorkingTimeTemp - finishWorkingTime).TotalHours;
            //                todo.Message += $"Tính toán lại replacementFrequency {old}: newreplacementFrequency: {recalculateReplacementFrequency} ";

            //            }
            //            var recalculateStandardConsumption = kgPair * (double)hourlyOutput * recalculateReplacementFrequency;
            //            todo.Consumption = recalculateStandardConsumption;

            //        }
            //    }
            //    replacementFrequency = 2;
            //    startWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
            //    list.Add(todo);
            //}
            //return list;
            throw new NotImplementedException();
        }

        public List<Response> TimeRange(DateTime start, DateTime end)
        {
            var lunchHour = 1;
            var ct = DateTime.Now;
            var startLunchTime = new DateTime(ct.Year, ct.Month, ct.Day, 12, 30, 0);
            var finishLunchTime = new DateTime(ct.Year, ct.Month, ct.Day, 13, 30, 0);
            var finishWorkingTime = end;
            var prepareTime = TimeSpan.FromMinutes(30);
            double replacementFrequency = 2;

            var startWorkingTimeTemp = start;
            var fwt = new DateTime();
            var kgPair = 0.01;
            var hourlyOutput = 120;
            var list = new List<Response>();
            while (true)
            {
                fwt = startWorkingTimeTemp.Add(prepareTime);
                var todo = new Response();
                if (startWorkingTimeTemp > end) break;
                //  12:30 >= SLT 12:30 and 13:30 <= FLT 13:30
                // TGBD ma lon hon TGBD An trua thi lay tu TGKT an trua tro di
                if (startWorkingTimeTemp >= startLunchTime && startWorkingTimeTemp <= finishLunchTime && fwt <= finishLunchTime || startWorkingTimeTemp > startLunchTime && startWorkingTimeTemp < finishLunchTime && fwt >= finishLunchTime)
                {
                    startWorkingTimeTemp = finishLunchTime; 
                    todo.Start = startWorkingTimeTemp;  // SLT 13:30
                    todo.End = startWorkingTimeTemp.Add(prepareTime); // 13:30 + preparetime
                    todo.Message += $"Giao với giờ ăn trưa: {replacementFrequency} ";
                    // 13:30 + 2 = 15:30 >= 14:00
                    var finishWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
                    if (finishWorkingTimeTemp >= finishWorkingTime)
                    {
                        replacementFrequency = (end - startWorkingTimeTemp).TotalHours;
                        todo.Message += $"Vượt quá endTime: {replacementFrequency}";
                    }
                    var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                    todo.Consumption = standardConsumption;

                }
                else
                {
                    //Neu TGBD < TGBD an trua && TGKT nam trong khoang TG An Trua va replacementFrequency > khoangTGAn trua thi tinh lai consumption
                    // 16:50 >= 16:30 -> 10minutes,
                    replacementFrequency = fwt >= finishWorkingTime ? (finishWorkingTime - startWorkingTimeTemp).TotalHours : replacementFrequency;
                   
                    // TGKT > TGKT Hanh Chinh thì tính lại consumption
                    if (fwt >= finishWorkingTime)
                    {
                        todo.Message += $"Thoi gian lam viec con lai co {replacementFrequency} gio. Tinh lai consumption";
                    }
                    todo.Start = startWorkingTimeTemp;
                    var standardConsumption = kgPair * (double)hourlyOutput * replacementFrequency;
                    todo.Consumption = standardConsumption;
                    todo.End = fwt;

                    // Nếu Cộng thêm replacementFrequency mà TG giao nhau voi TG an trua thi phai tru ra TG an trua
                    var finishWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
                    // EX: StartTime của nhiệm vụ tiếp theo là 13:30 , thì khoảng từ 11:30 -> 13:30 sẽ bị giao với TG ăn trua
                    // 12:30 >= 11:30 and 9:30 >= 13:30
                    if (startLunchTime >= startWorkingTimeTemp && finishWorkingTimeTemp >= finishLunchTime)
                    {
                        var recalculateReplacementFrequency = replacementFrequency - lunchHour;
                        todo.Message += $"{startLunchTime.ToString("HH:mm")} >= {startWorkingTimeTemp.ToString("HH:mm")} && {finishWorkingTimeTemp.ToString("HH:mm")} >= {finishLunchTime.ToString("HH:mm")}";

                        // Nếu FWTT 14:20 > FWT 14:00-> dư ra 20 phút thì phải trừ ra 20 phút
                        if (finishWorkingTimeTemp >= finishWorkingTime)
                        {
                            var old = recalculateReplacementFrequency;
                            recalculateReplacementFrequency = recalculateReplacementFrequency - (finishWorkingTimeTemp - finishWorkingTime).TotalHours;
                            todo.Message += $"Tính toán lại replacementFrequency {old}: newreplacementFrequency: {recalculateReplacementFrequency} ";

                        }
                        var recalculateStandardConsumption = kgPair * (double)hourlyOutput * recalculateReplacementFrequency;
                        todo.Consumption = recalculateStandardConsumption;

                    }
                }
                replacementFrequency = 2;
                startWorkingTimeTemp = startWorkingTimeTemp.AddHours(replacementFrequency);
                list.Add(todo);
            }
            return list;
        }
    }
}
