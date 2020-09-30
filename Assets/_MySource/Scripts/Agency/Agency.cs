using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Server.Api
{
    public class Agency
    {
        private string fullName;
        private string nickName;
        private string phone;
        private string address;
        private string domain;

        public string _fullName
        {
            get
            {
                return fullName;
            }

            set
            {
                fullName = value;
            }
        }

        public string _nickName
        {
            get
            {
                return nickName;
            }

            set
            {
                nickName = value;
            }
        }

        public string _phone
        {
            get
            {
                return phone;
            }

            set
            {
                phone = value;
            }
        }

        public string _address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }

        public string _domain
        {
            get
            {
                return domain;
            }

            set
            {
                domain = value;
            }
        }

        public Agency()
        {

        }
        public Agency(string fullName, string nickName, string phone, string address, string domain)
        {
            this._fullName = fullName;
            this._nickName = nickName;
            this._phone = phone;
            this._address = address;
            this._domain = domain;
        }
    }
}