using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using GuaranteedRate;

namespace GuaranteedRateWebApi.Controllers
{
    [RoutePrefix("api/records")]
    public class RecordsController : ApiController
    {
        private ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private static string filePath = string.Format(@"{0}/TestFile.txt", HttpContext.Current.Server.MapPath("/bin"));

        [Route("gender")]
        public async Task<IHttpActionResult> GetRecordsSortedByGender()
        {
            var records = await GetRecordsAsync();
            if (records == null)
            {
                return InternalServerError();
            }
            return Ok(records.OrderByDescending(p => p.Gender));
        }

        [Route("birthdate")]
        public async Task<IHttpActionResult> GetRecordsSortedByBirthDate()
        {
            var records = await GetRecordsAsync();
            if (records == null)
            {
                return InternalServerError();
            }
            return Ok(records.OrderBy(p => p.DateOfBirth));
        }

        [Route("name")]
        public async Task<IHttpActionResult> GetRecordsSortedByName()
        {
            var records = await GetRecordsAsync();
            if (records == null)
            {
                return InternalServerError();
            }
            return Ok(records.OrderBy(p => p.LastName).ThenBy(p => p.FirstName));
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]string record)
        {
            string[] entries;
            if (!DelimitedSerializer<Person>.TryParseRecord(record, out entries))
            {
                return Conflict();
            }
            try
            {
                locker.EnterWriteLock();
                using (TextWriter textWriter = new StreamWriter(filePath, true))
                {
                    await textWriter.WriteLineAsync(record);
                }
                return Ok();
            }
            catch (Exception)
            {
                //log exception in the log file + consider sending email to all interested developers
                //somebody may like rethrow this exception(return it via function parameters) and handle it later + return error in HttpResponse
                //it depends. My preference is to log + send email
                return InternalServerError();
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        private async Task<IEnumerable<Person>> GetRecordsAsync()
        {
            try
            {
                locker.EnterReadLock();
                using (TextReader textReader = new StreamReader(filePath))
                {
                    DelimitedSerializer<Person> serializer = new DelimitedSerializer<Person>();
                    return await serializer.DeserializeAsync(textReader);
                }
            }
            catch(Exception ex)
            {
                //log exception in the log file + consider sending email to all interested developers
                //somebody may like rethrow this exception(return it via function parameters) and handle it later + return error in HttpResponse
                //it depends. My preference is to log + send email
                return null;
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }
}
