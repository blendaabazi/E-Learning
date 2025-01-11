using E_Learning.Data;
using E_Learning.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace E_Learning.Controllers
{
    public class CacheController : ControllerBase
    {
        private readonly RedisCacheService _redisCache;

        public CacheController(RedisCacheService redisCache)
        {
            _redisCache = redisCache;
        }


        [HttpGet("test-redis")]
        public async Task<IActionResult> TestRedis()
        {
            var testCacheKey = "test";
            await _redisCache.SetValueAsync(testCacheKey, "This is a test");

            var testValue = await _redisCache.GetValueAsync(testCacheKey);

            if (string.IsNullOrEmpty(testValue))
            {
                return BadRequest("No value found in Redis.");
            }

            return Ok(new { message = "Value from Redis: " + testValue });
        }


        [HttpGet("check-redis")]
        public async Task<IActionResult> CheckRedisCache()
        {
            var trainingsCacheKey = "trainings";
            var trainings = await _redisCache.GetValueAsync(trainingsCacheKey);

            if (string.IsNullOrEmpty(trainings))
            {
                // Nëse nuk ka të dhëna, kthejmë një mesazh bosh, por me statusin 200 OK
                return Ok("No training data found in cache.");
            }
            else
            {
                // Deserializojmë të dhënat JSON dhe marrim vetëm listën e trajnimeve
                var trainingList = JsonConvert.DeserializeObject<List<Training>>(trainings);

                // Kthejmë vetëm listën e trajnimeve si pjesë e trupit të përgjigjes
                return Ok(new { trainings = trainingList });
            }
        }



        [HttpPost("set")]
        public async Task<IActionResult> SetCache(string key, string value)
        {
            await _redisCache.SetValueAsync(key, value);
            return Ok("Value set in Redis");
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCache(string key)
        {
            var value = await _redisCache.GetValueAsync(key);
            if (value == null)
            {
                return NotFound("Key not found in Redis");
            }
            return Ok(value);
        }
    }
}
