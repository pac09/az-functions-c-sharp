using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMusicWorld.Data;
using System.Collections;

namespace ServerlessMusicWorld;

public static class Functions
{
    [FunctionName("CreateArtist")]
    public static async Task<IActionResult> CreateArtist([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "artist")] HttpRequest request, ILogger log)
    {
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        var dtoArtist = JsonConvert.DeserializeObject<Artist>(requestBody);

        try
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));

            connection.Open();
            if (!string.IsNullOrEmpty(dtoArtist.Name))
            {
                var query = $"INSERT INTO [dbo].[Artist] ([Name],[Nationality],[Founded]) VALUES('{dtoArtist.Name}', '{dtoArtist.Nationality}', '{dtoArtist.Founded}')";
                var command = new SqlCommand(query, connection);
                command.ExecuteNonQuery();
            }

        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
            return new BadRequestResult();
        }

        return new OkResult();
    }

    [FunctionName("GetArtists")]
    public static async Task<IActionResult> GetArtists([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "artist")] HttpRequest request, ILogger log)
    {
        var artists = new List<Artist>();

        try
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
            connection.Open();

            var query = @"SELECT * from dbo.Artist";
            var command = new SqlCommand(query, connection);
            
            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var artist = new Artist()
                {
                    ArtistId = (int)reader["ArtistId"],
                    Name = reader["Name"].ToString(),
                    Nationality = reader["Nationality"].ToString(),
                    Founded = (long)reader["Founded"]
                };

                artists.Add(artist);
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
        }

        if (artists.Count > 0)
            return new OkObjectResult(artists);
        else
            return new NotFoundResult();
    }


    [FunctionName("GetArtistById")]
    public static async Task<IActionResult> GetArtistById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "artist/{id}")] HttpRequest request, ILogger log, int id)
    {
        var dataTable = new DataTable();

        try
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
            connection.Open();

            var query = @"SELECT * FROM dbo.Artist WHERE ArtistId = @Id";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            var dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(dataTable);
        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
        }

        if (dataTable.Rows.Count == 0)
            return new NotFoundResult(); 
        else
            return new OkObjectResult(dataTable);
    }

    [FunctionName("UpdateArtist")]
    public static async Task<IActionResult> UpdateArtist([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "artist/{id}")] HttpRequest request, ILogger log, int id)
    {
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();
        var dtoArtist = JsonConvert.DeserializeObject<Artist>(requestBody);

        try
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
            connection.Open();

            var query = @"UPDATE dbo.Artist SET [Name] = @Name, [Nationality] = @Nationality, [Founded] = @Founded WHERE ArtistId = @Id";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", dtoArtist.Name);
            command.Parameters.AddWithValue("@Nationality", dtoArtist.Nationality);
            command.Parameters.AddWithValue("@Founded", dtoArtist.Founded);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
        }

        return new OkResult();
    }

    [FunctionName("DeleteArtist")]
    public static async Task<IActionResult> DeleteArtist([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "artist/{id}")] HttpRequest request, ILogger log, int id)
    {
        try
        {
            using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
            connection.Open();

            var query = @"DELETE FROM dbo.Artist WHERE ArtistId = @Id";
            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
            return new BadRequestResult();
        }

        return new OkResult();
    }

}