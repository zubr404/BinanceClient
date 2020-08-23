using DataBaseWork.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataBaseWork.Repositories
{
    public class APIKeyRepository
    {
        readonly DataBaseContext db;
        public APIKeyRepository(DataBaseContext db)
        {
            this.db = db;
        }

        public bool ExistsName(string apiKeyName)
        {
            if (!string.IsNullOrWhiteSpace(apiKeyName))
            {
                return db.APIKeys.AsNoTracking().Any(x => x.Name == apiKeyName);
            }
            else
            {
                throw new ArgumentException("Параметр не содержит допустимого значения.", "name");
            }
        }

        public bool ExistsPublicKey(string publicKey)
        {
            if (!string.IsNullOrWhiteSpace(publicKey))
            {
                return db.APIKeys.AsNoTracking().Any(x => x.PublicKey == publicKey);
            }
            else
            {
                throw new ArgumentException("Параметр не содержит допустимого значения.", "publicKey");
            }
        }

        public bool ExistsSecretKey(string secretKey)
        {
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                return db.APIKeys.AsNoTracking().Any(x => x.SecretKey == secretKey);
            }
            else
            {
                throw new ArgumentException("Параметр не содержит допустимого значения.", "secretKey");
            }
        }

        public IEnumerable<APIKey> Get()
        {
            return db.APIKeys;
        }

        public APIKey Get(string apiKeyName)
        {
            return db.APIKeys.FirstOrDefault(u => u.Name == apiKeyName);
        }

        public APIKey Update(APIKey keyItem)
        {
            if (!string.IsNullOrWhiteSpace(keyItem.Name) && !string.IsNullOrWhiteSpace(keyItem.PublicKey) && !string.IsNullOrWhiteSpace(keyItem.SecretKey))
            {
                if (ExistsName(keyItem.Name))
                {
                    if (ExistsPublicKey(keyItem.PublicKey))
                    {
                        throw new ArgumentException("Параметр содержит уже имеющийся публичный ключ.", "keyItem");
                    }
                    if (ExistsSecretKey(keyItem.SecretKey))
                    {
                        throw new ArgumentException("Параметр содержит уже имеющийся секретный ключ.", "keyItem");
                    }
                    var apikey = db.APIKeys.FirstOrDefault(x => x.Name == keyItem.Name);
                    if (apikey != null)
                    {
                        apikey.PublicKey = keyItem.PublicKey;
                        apikey.SecretKey = keyItem.SecretKey;
                        Save();
                        return apikey;
                    }
                    return null;
                }
                else
                {
                    try
                    {
                        if (ExistsPublicKey(keyItem.PublicKey))
                        {
                            throw new ArgumentException("Параметр содержит уже имеющийся публичный ключ.", "keyItem");
                        }
                        if (ExistsSecretKey(keyItem.SecretKey))
                        {
                            throw new ArgumentException("Параметр содержит уже имеющийся секретный ключ.", "keyItem");
                        }
                        var apikey = db.APIKeys.Add(keyItem);
                        Save();
                        return apikey.Entity;
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw ex;
                    }
                }
            }
            else
            {
                throw new ArgumentException("Параметр содержит не допустимые значения.", "keyItem");
            }
        }

        private void Save()
        {
            db.SaveChanges();
        }
    }
}
