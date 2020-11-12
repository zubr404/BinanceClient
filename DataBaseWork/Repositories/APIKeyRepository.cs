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
        public bool ExistsName(string apiKeyName)
        {
            if (!string.IsNullOrWhiteSpace(apiKeyName))
            {
                using (var db = new DataBaseContext())
                {
                    return db.APIKeys.AsNoTracking().Any(x => x.Name == apiKeyName);
                }
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
                using (var db = new DataBaseContext())
                {
                    return db.APIKeys.AsNoTracking().Any(x => x.PublicKey == publicKey);
                }
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
                using (var db = new DataBaseContext())
                {
                    return db.APIKeys.AsNoTracking().Any(x => x.SecretKey == secretKey);
                }
            }
            else
            {
                throw new ArgumentException("Параметр не содержит допустимого значения.", "secretKey");
            }
        }

        public IEnumerable<APIKey> Get()
        {
            using (var db = new DataBaseContext())
            {
                var keys = db.APIKeys.AsNoTracking().ToArray();
                return keys;
            }
        }

        public IEnumerable<APIKey> GetActive()
        {
            using (var db = new DataBaseContext())
            {
                return db.APIKeys.AsNoTracking().Where(x => x.IsActive).ToArray();
            }
        }

        public IEnumerable<APIKey> GetActiveStatusOk()
        {
            using (var db = new DataBaseContext())
            {
                return db.APIKeys.AsNoTracking().Where(x => x.IsActive && x.Status).ToArray();
            }
        }

        public string GetSecretKey(string publicKey)
        {
            using (var db = new DataBaseContext())
            {
                return db.APIKeys.AsNoTracking().Where(x => x.PublicKey == publicKey).Select(x => x.SecretKey).FirstOrDefault();
            }
        }

        public string GetNameKey(string publicKey)
        {
            using (var db = new DataBaseContext())
            {
                return db.APIKeys.AsNoTracking().Where(x => x.PublicKey == publicKey).Select(x => x.Name).FirstOrDefault();
            }
        }

        public APIKey Get(string apiKeyName)
        {
            using (var db = new DataBaseContext())
            {
                return db.APIKeys.FirstOrDefault(u => u.Name == apiKeyName);
            }
        }

        public APIKey UpdateActive(int Id, bool isActive)
        {
            using (var db = new DataBaseContext())
            {
                var key = db.APIKeys.FirstOrDefault(x => x.ID == Id);
                if (key != null)
                {
                    key.IsActive = isActive;
                    db.SaveChanges();
                }
                return key;
            }
        }

        public APIKey UpdateStatus(string publicKey, bool status)
        {
            using (var db = new DataBaseContext())
            {
                var key = db.APIKeys.FirstOrDefault(x => x.PublicKey == publicKey);
                if (key != null)
                {
                    key.Status = status;
                    db.SaveChanges();
                }
                return key;
            }
        }

        public APIKey Update(APIKey keyItem)
        {
            using (var db = new DataBaseContext())
            {
                var apikey = db.APIKeys.FirstOrDefault(x => x.Name == keyItem.Name);
                if (apikey != null)
                {
                    apikey.PublicKey = keyItem.PublicKey;
                    apikey.SecretKey = keyItem.SecretKey;
                    apikey.IsActive = keyItem.IsActive;
                    apikey.Status = keyItem.Status;
                    db.SaveChanges();
                    return apikey;
                }
                return null;
            }
        }

        public APIKey Create(APIKey keyItem)
        {
            if (!string.IsNullOrWhiteSpace(keyItem.Name) && !string.IsNullOrWhiteSpace(keyItem.PublicKey) && !string.IsNullOrWhiteSpace(keyItem.SecretKey))
            {
                if (ExistsName(keyItem.Name))
                {
                    throw new ArgumentException("Параметр содержит уже имеющееся имя пользователя.", "keyItem");
                    // по публичному ключу привязываются сделки, поэтому что ниже отменил.
                    if (ExistsPublicKey(keyItem.PublicKey))
                    {
                        throw new ArgumentException("Параметр содержит уже имеющийся публичный ключ.", "keyItem");
                    }
                    if (ExistsSecretKey(keyItem.SecretKey))
                    {
                        throw new ArgumentException("Параметр содержит уже имеющийся секретный ключ.", "keyItem");
                    }
                    using (var db = new DataBaseContext())
                    {
                        var apikey = db.APIKeys.FirstOrDefault(x => x.Name == keyItem.Name);
                        if (apikey != null)
                        {
                            apikey.PublicKey = keyItem.PublicKey;
                            apikey.SecretKey = keyItem.SecretKey;
                            db.SaveChanges();
                            return apikey;
                        }
                        return null;
                    }
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
                        using (var db = new DataBaseContext())
                        {
                            var apikey = db.APIKeys.Add(keyItem);
                            db.SaveChanges();
                            return apikey.Entity;
                        }
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
    }
}
