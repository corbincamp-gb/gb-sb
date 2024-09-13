using System.Text;
using SkillBridge.Business.Mapping;
using SkillBridge.Business.Model;
using Taku.Core;

namespace SkillBridge.Business.Query.DataDownload
{
    public interface IDropdownDataQuery : IQuery
    {
        IDropDownData Get();
    }

    public class DropdownDataQuery : IDropdownDataQuery
    {
        private readonly IDropDownDataMapping _mapping;
        private readonly IMilitaryBranchCollectionQuery _militaryBranchCollectionQuery;
        private readonly IOpportunityCollectionQuery _opportunityCollectionQuery;
        private readonly IProgramOrganizationCollectionQuery _programOrganizationCollectionQuery;
        private readonly IRelatedOrganizationCollectionQuery _relatedOrganizationCollectionQuery;

        public DropdownDataQuery(IDropDownDataMapping mapping, 
            IMilitaryBranchCollectionQuery militaryBranchCollectionQuery,
            IOpportunityCollectionQuery opportunityCollectionQuery,
            IProgramOrganizationCollectionQuery programOrganizationCollectionQuery,
            IRelatedOrganizationCollectionQuery relatedOrganizationCollectionQuery)
        {
            _mapping = mapping;
            _militaryBranchCollectionQuery = militaryBranchCollectionQuery;
            _opportunityCollectionQuery = opportunityCollectionQuery;
            _programOrganizationCollectionQuery = programOrganizationCollectionQuery;
            _relatedOrganizationCollectionQuery = relatedOrganizationCollectionQuery;
        }
        public IDropDownData Get()
        {
            // start refactor

            var ret = _mapping.Map(
                _militaryBranchCollectionQuery.Get(),
                _opportunityCollectionQuery.Get(),
                _programOrganizationCollectionQuery.Get(),
                _relatedOrganizationCollectionQuery.Get());

            return ret;

            #region old code

            //var orgs = _db.Organizations.AsNoTracking();
            //var progs = _db.Programs.AsNoTracking();
            //var opps = _db.Opportunities.AsNoTracking();


            //// Generate the string of JSON
            ////string newJson = "var locations = { data: [";
            //StringBuilder newJson = new StringBuilder("");

            //try
            //{
            //    /* PROGRAMS/PROVIDERS */
            //    // Get Unique Programs
            //    var uniquePrograms = progs.FromCache().OrderBy(a => a.Program_Name).ToList();
            //    // Sort Alphebetically
            //    //uniquePrograms.Sort();

            //    string uniqueProgramsForExport = "const programDropdown = new Array(";

            //    int numOutput = 0;

            //    for (var i = 0; i < uniquePrograms.Count; i++)
            //    {
            //        var progList = progs.FromCache().Where(m => m.Organization_Id == uniquePrograms[i].Organization_Id).ToList();
            //        var oppList = opps.FromCache().Where(m => m.Program_Id == uniquePrograms[i].Id).ToList();
            //        Console.WriteLine("Program '" + uniquePrograms[i].Program_Name + "' has " + oppList.Count + " Opportunities attached to it");

            //        bool soloProgramUnderOrg = true;

            //        if (progList.Count > 1)
            //        {
            //            soloProgramUnderOrg = false;
            //        }

            //        bool hasActiveOpp = false;

            //        for (var j = 0; j < oppList.Count; j++)
            //        {
            //            if (oppList[j].Is_Active == true)
            //            {
            //                hasActiveOpp = true;
            //            }
            //        }

            //        //check to see how many programs in each org, if only one then dont output the org name with hyphen

            //        if (oppList.Count > 0 && hasActiveOpp == true && uniquePrograms[i].Is_Active == true)
            //        {
            //            var orgName = uniquePrograms[i].Organization_Name;//.Replace("'", @"\'");
            //            var progName = uniquePrograms[i].Program_Name;//.Replace("'", @"\'");

            //            if (numOutput == 0)
            //            {
            //                if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
            //                {
            //                    uniqueProgramsForExport += "\"" + orgName + " - " + progName + "\"";
            //                }
            //                else
            //                {
            //                    uniqueProgramsForExport += "\"" + progName + "\"";
            //                }
            //            }
            //            else
            //            {
            //                if (soloProgramUnderOrg == false || uniquePrograms[i].Program_Name != uniquePrograms[i].Organization_Name)
            //                {
            //                    uniqueProgramsForExport += ", \"" + orgName + " - " + progName + "\"";
            //                }
            //                else
            //                {
            //                    uniqueProgramsForExport += ", \"" + progName + "\"";
            //                }
            //            }
            //            numOutput++;
            //        }
            //    }

            //    uniqueProgramsForExport += ");";

            //    Console.WriteLine("numOutput: " + numOutput);

            //    // Add to main body of data to export
            //    newJson.Append(uniqueProgramsForExport + "\n");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("e: " + e.Message);
            //}

            ///* SERVICES */
            //newJson.Append("const serviceDropdown = new Array(\"Air Force\", \"Army\", \"Coast Guard\", \"Marine Corps\", \"Navy\");");

            ///* DURATION OF TRAINING */
            //// Get Unique Durations
            //var uniqueDurations = _db.Opportunities.Select(m => m.Training_Duration).Distinct().ToList();
            //List<string> durationsList = new List<string>();
            //List<string> uniqueDurationsList = new List<string>();

            //for (var i = 0; i < uniqueDurations.Count; i++)
            //{
            //    var splits = uniqueDurations[i].Split(",");

            //    for (var x = 0; x < splits.Length; x++)
            //    {
            //        var item = splits[x].Trim();
            //        if (item != "" && item != " ")
            //        {
            //            durationsList.Add(item);
            //        }
            //    }
            //}

            //// Get Unique Values from extracted values
            //uniqueDurationsList.AddRange(durationsList.Distinct());

            //// Sort new list alphabetically using custom comparer that will sort on both alpha and numbers
            ////var myComparer = new NumberComparer();
            ////uniqueDurationsList.Sort(myComparer);

            //// Sort Alphebetically
            ////var collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base'});
            ////uniqueDurations.sort(collator.compare);

            //// sort can happen on the JS side since it's easier

            //var uniqueDurationsForExport = "const durationDropdown = new Array(";

            //for (var i = 0; i< uniqueDurationsList.Count; i++)
            //{
            //    if (i==0)
            //    {
            //        uniqueDurationsForExport += "'" + uniqueDurationsList[i].Replace("'", @"\'") + "'";
            //    }
            //    else
            //    {
            //        uniqueDurationsForExport += ", '" + uniqueDurationsList[i].Replace("'", @"\'") + "'";
            //    }
            //}

            //uniqueDurationsForExport += ");";
            ////console.log("uniqueDurationsForExport: " + uniqueDurationsForExport);
            //newJson.Append(uniqueDurationsForExport + "\n");

            ///* DELIVERY METHOD */
            //// Get Unique Programs
            //List<string> uniqueDeliveryMethods = _db.Opportunities.Select(m => m.Delivery_Method).Distinct().ToList();
            //// Sort Alphebetically
            //uniqueDeliveryMethods.Sort();

            //var uniqueDeliveryMethodsForExport = "const deliveryDropdown = new Array(";

            //for (var i = 0; i < uniqueDeliveryMethods.Count; i++)
            //{
            //    string newDM = "";

            //    if (uniqueDeliveryMethods[i] == "0")
            //    {
            //        newDM = "In-person";
            //    }
            //    else if (uniqueDeliveryMethods[i] == "1")
            //    {
            //        newDM = "Online";
            //    }
            //    else if (uniqueDeliveryMethods[i] == "2")
            //    {
            //        newDM = "Hybrid (In-Person and Online)";
            //    }


            //    if (i == 0)
            //    {
            //        uniqueDeliveryMethodsForExport += "'" + newDM + "'";
            //    }
            //    else
            //    {
            //        uniqueDeliveryMethodsForExport += ", '" + newDM + "'";
            //    }
            //}

            //uniqueDeliveryMethodsForExport += ");";

            //newJson.Append(uniqueDeliveryMethodsForExport + "\n");




            //// Get Unique Locations
            //List<string> uniqueLocationItems = _db.Opportunities.Select(m => m.Locations_Of_Prospective_Jobs_By_State).Distinct().ToList();
            //// Sort Alphebetically
            //uniqueLocationItems.Sort();

            //List<string> locationsList = new List<string>();
            //List<string> uniqueLocations = new List<string>();

            //for (var i = 0; i < uniqueLocationItems.Count; i++)
            //{
            //    if (uniqueLocationItems[i] != null)
            //    {
            //        //Console.WriteLine("uniqueLocationItems[i]: " + uniqueLocationItems[i]);
            //        var splits = uniqueLocationItems[i].Split(",");

            //        for (var x = 0; x < splits.Length; x++)
            //        {
            //            var item = splits[x].Trim();
            //            if (item != "" && item != " " && item != "All Services")
            //            {
            //                locationsList.Add(item);
            //            }

            //        }
            //    }
            //}

            //// Get Unique Values from extracted values
            //uniqueLocations.AddRange(locationsList.Distinct());

            //// Sort Alphebetically
            //uniqueLocations.Sort();

            //var uniqueLocationsForExport = "const locationDropdown = new Array(";

            //for (var i = 0; i < uniqueLocations.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        uniqueLocationsForExport += "'" + uniqueLocations[i] + "'";
            //    }
            //    else
            //    {
            //        uniqueLocationsForExport += ", '" + uniqueLocations[i] + "'";
            //    }

            //}

            //uniqueLocationsForExport += ");";

            //newJson.Append(uniqueLocationsForExport + "\n");


            //// Get Unique Job Families
            //List<string> uniqueJobFamilyItems = _db.Opportunities.Select(m => m.Job_Families).Distinct().ToList();
            //// Sort Alphebetically
            //uniqueJobFamilyItems.Sort();

            //List<string> jobFamilyList = new List<string>();
            //List<string> uniqueJobFamilies = new List<string>();

            //for (var i = 0; i < uniqueJobFamilyItems.Count; i++)
            //{
            //    var splits = uniqueJobFamilyItems[i].Split(";");

            //    for (var x = 0; x < splits.Length; x++)
            //    {
            //        var item = splits[x].Trim();
            //        if (item != "" && item != " " && item != "All Services")
            //        {
            //            jobFamilyList.Add(item);
            //        }

            //    }
            //}

            //// Get Unique Values from extracted values
            //uniqueJobFamilies.AddRange(jobFamilyList.Distinct());

            //// Sort Alphebetically
            //uniqueJobFamilies.Sort();

            //var uniqueJobFamiliesForExport = "const familyDropdown = new Array(";

            //for (var i = 0; i < uniqueJobFamilies.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        uniqueJobFamiliesForExport += "'" + uniqueJobFamilies[i] + "'";
            //    }
            //    else
            //    {
            //        uniqueJobFamiliesForExport += ", '" + uniqueJobFamilies[i] + "'";
            //    }

            //}

            //uniqueJobFamiliesForExport += ");";

            //newJson.Append(uniqueJobFamiliesForExport + "\n");









            ///* COMPANY */
            //// Get Unique Companies
            //List<string> uniqueParentOrgItems = _db.Opportunities.Select(m => m.Organization_Name).Distinct().ToList();
            //List<int> uniqueParentOrgIds = _db.Opportunities.Select(m => m.Organization_Id).Distinct().ToList();
            ////console.log("uniqueParentOrgItems.length: " + uniqueParentOrgItems.length);
            //List<string> parentOrgList = new List<string>();
            //List<string> uniqueParentOrgs = new List<string>();

            //// Sort Alphebetically
            //uniqueParentOrgItems.Sort();

            //for (var i = 0; i < uniqueParentOrgItems.Count; i++)
            //{
            //    //console.log("uniqueParentOrgItems[i].ORGANIZATION: " + uniqueParentOrgItems[i].PARENTORGANIZATION);
            //    parentOrgList.Add(uniqueParentOrgItems[i]);
            //}

            //// Get Unique Values from extracted values
            //uniqueParentOrgs.AddRange(parentOrgList.Distinct());

            //// Sort Alphebetically
            //uniqueParentOrgs.Sort();

            //var uniqueParentOrgsForExport = "const parentOrgDropdown = new Array(";

            //for (var i = 0; i < uniqueParentOrgs.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        uniqueParentOrgsForExport += "'" + uniqueParentOrgs[i].Replace("'", @"\'") + "'";
            //    }
            //    else
            //    {
            //        uniqueParentOrgsForExport += ", '" + uniqueParentOrgs[i].Replace("'", @"\'") + "'";
            //    }

            //}

            //uniqueParentOrgsForExport += ");";
            ////console.log("uniqueParentOrgsForExport: " + uniqueParentOrgsForExport);
            //newJson.Append(uniqueParentOrgsForExport + "\n");


            //var relatedOrgsForExport = "var relatedOrgs = { data: [";

            //// Find all Orgs under each parent org
            //for (var i = 0; i < uniqueParentOrgs.Count; i++)
            //{
            //    //var orgItems = _.where(data, { PARENTORGANIZATION: uniqueParentOrgs[i]});
            //    List<string> orgItems = _db.Organizations.Where(m => m.Parent_Organization_Name == uniqueParentOrgs[i]).Select(m => m.Name).ToList();

            //    // Get Unique Values from extracted values
            //    List<string> uniqueOrgItems = new List<string>();
            //    uniqueOrgItems.AddRange(orgItems.Distinct());
            //    // Sort Alphebetically
            //    uniqueOrgItems.Sort();

            //    // If multiple orgs under parent org, use opt group
            //    if (uniqueOrgItems.Count > 1)
            //    {
            //        if (i > 0)
            //        {
            //            relatedOrgsForExport += ",";
            //        }
            //        relatedOrgsForExport += "{ 'parentOrg': '" + uniqueParentOrgs[i].Replace("'", @"\'") + "','orgs':[";

            //        for (var j = 0; j < uniqueOrgItems.Count; j++)
            //        {
            //            if (j > 0)
            //            {
            //                relatedOrgsForExport += ",";
            //            }
            //            relatedOrgsForExport += "'" + uniqueOrgItems[j].Replace("'", @"\'") + "'";
            //        }

            //        relatedOrgsForExport += "]}";
            //    }
            //    else
            //    {
            //        if (i > 0)
            //        {
            //            relatedOrgsForExport += ",";
            //        }
            //        relatedOrgsForExport += "{ 'parentOrg': '" + uniqueParentOrgs[i].Replace("'", @"\'") + "','orgs':[";
            //        for (var j = 0; j < uniqueOrgItems.Count; j++)
            //        {
            //            if (j > 0)
            //            {
            //                relatedOrgsForExport += ",";
            //            }
            //            relatedOrgsForExport += "'" + uniqueOrgItems[j].Replace("'", @"\'") + "'";
            //        }
            //        relatedOrgsForExport += "]}";
            //    }
            //}

            //relatedOrgsForExport += "]}";

            //newJson.Append(relatedOrgsForExport + "\n");

            //return newJson.ToString();

            #endregion

        }
    }
}
