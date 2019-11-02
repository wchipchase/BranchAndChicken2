using System;
using System.Collections.Generic;
using System.Linq;
using BranchAndChicken.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;

namespace BranchAndChicken.Api.DataAccess
{
    public class TrainerRepository
    {
        string _connectionString = "Server=localhost;Database=BranchAndChicken;Trusted_Connection=True;";

        public List<Trainer> GetAll()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                db.Open();
                var trainers = db.Query<Trainer>("Select * From Trainer");

                return trainers.AsList();
            }
        }

        public Trainer Get(string name)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = @"select *
                            from Trainer
                            where Trainer.Name = @trainerName";

                var trainer = db.QueryFirst<Trainer>(sql, new {trainerName = name});
                return trainer;

            }

        }

        public bool Remove(string name)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = @"delete 
                            from trainer 
                            where [name] = @name";

                return db.Execute(sql, new {name}) == 1;

            }

        }

        public ActionResult<Trainer> GetSpecialty(string specialty)
        {
            throw new NotImplementedException();
        }

        public Trainer Update(Trainer updatedTrainer, int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"UPDATE [Trainer]
                                            SET [Name] = @name
                                          ,[YearsOfExperience] = @yearsOfExperience
                                          ,[Specialty] = @specialty
                                        output inserted.*
                                        WHERE id = @id";

                cmd.Parameters.AddWithValue("name", updatedTrainer.Name);
                cmd.Parameters.AddWithValue("yearsOfExperience", updatedTrainer.YearsOfExperience);
                cmd.Parameters.AddWithValue("specialty", updatedTrainer.Specialty);
                cmd.Parameters.AddWithValue("id", id);

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return GetTrainerFromDataReader(reader);
                }

                return null;
            }
        }

        public Trainer Add(Trainer newTrainer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"INSERT INTO [dbo].[Trainer]
                                       ([Name]
                                       ,[YearsOfExperience]
                                       ,[Specialty])
                                    output inserted.*
                                    VALUES
                                       (@name
                                       ,@yearsOfExperience
                                       ,@specialty)";

                cmd.Parameters.AddWithValue("name", newTrainer.Name);
                cmd.Parameters.AddWithValue("yearsOfExperience", newTrainer.YearsOfExperience);
                cmd.Parameters.AddWithValue("specialty", newTrainer.Specialty);


                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return GetTrainerFromDataReader(reader);
                }

                return null;
            }
            //_trainers.Add(newTrainer);
            return newTrainer;
        }

        Trainer GetTrainerFromDataReader(SqlDataReader reader)
        {
            //explicit cast
            var id = (int)reader["Id"];
            //implicit cast
            var returnedName = reader["name"] as string;
            //convert to
            var yearsOfExperience = Convert.ToInt32(reader["YearsOfExperience"]);
            //try parse
            Enum.TryParse<Specialty>(reader["specialty"].ToString(), out var specialty);

            var trainer = new Trainer
            {
                Specialty = specialty,
                Id = id,
                Name = returnedName,
                YearsOfExperience = yearsOfExperience
            };

            return trainer;
        }
    }
}
