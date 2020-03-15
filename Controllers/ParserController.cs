using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaspAPI.Models;
using RaspAPI.Parsers;

namespace RaspAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParserController : ControllerBase
    {
        public ParserController()
        {

        }
        [HttpGet("{loadTestData?}")]
        public List<Facility> Get(bool loadTestData = true)
        {
            IQueryable<Facility> data;
            if (!loadTestData)
            {
                data = ApplicationContext.Instance.Facilities.Where(x => !x.TestOnly);
            }
            else
            {
                data = ApplicationContext.Instance.Facilities;
            }
            return data.ToList();

        }
        //[HttpGet("{type}")]
        [HttpGet("schedule/{type}")]
        public async Task<List<GroupInfo>> Get(int type)
        {
            ScheduleParser parser;

            switch (type)
            {
                case 0:
                    parser = new AuesParser();
                    break;
                case 1:
                    parser = new KaznauParser();
                    break;
                case 2:
                    parser = new IneuParser();
                    break;
                case 100:
                    parser = new AstuParser();
                    break;
                default:
                    throw new Exception("Incorrect type");
            }
            return await parser.ParseGroupList();

        }
        //[HttpGet("{type}/{index}")]
        [HttpGet("schedule/{type}/{index}")]
        public async Task<Schedule> Get(int type, string index)
        {
            ScheduleParser parser;
            var f = ApplicationContext.Instance.Facilities.Where(x => x.Index == type).FirstOrDefault();
            switch (type)
            {
                case 0:
                    parser = new AuesParser();
                    break;
                case 1:
                    parser = new KaznauParser();
                    break;
                case 2:
                    parser = new IneuParser();
                    break;
                case 100:
                    parser = new AstuParser();
                    break;
                default:
                    throw new Exception("Incorrect type");
            }
            var s = await parser.ParseSchedule(index);
            s.Facility = f;
            return s;
        }
    }
}