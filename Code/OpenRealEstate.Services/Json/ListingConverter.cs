﻿using System;
using Newtonsoft.Json.Linq;
using OpenRealEstate.Core.Models;
using OpenRealEstate.Core.Models.Land;
using OpenRealEstate.Core.Models.Rental;
using OpenRealEstate.Core.Models.Residential;
using OpenRealEstate.Core.Models.Rural;

namespace OpenRealEstate.Services.Json
{
    public class ListingConverter : JsonCreationConverter<Listing>
    {
        protected override Listing Create(Type objectType, JObject jObject)
        {
            const string fieldName = "ListingType";

            var value = jObject[fieldName]?.ToString();
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception(
                    $"Failed to find the field '{fieldName}' which is expected, so we know which type of Listing instance to deserialize the data into.");
            }

            if (value.Equals("Residential", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ResidentialListing();
            }

            if (value.Equals("Rental", StringComparison.InvariantCultureIgnoreCase))
            {
                return new RentalListing();
            }

            if (value.Equals("Land", StringComparison.InvariantCultureIgnoreCase))
            {
                return new LandListing();
            }

            if (value.Equals("Rural", StringComparison.InvariantCultureIgnoreCase))
            {
                return new RuralListing();
            }

            throw new Exception(
                $"Invalid value found in the expected field '{fieldName}'. Only the following values (ie. listing types) as supported: residential, rental, land or rural.");
        }
    }
}