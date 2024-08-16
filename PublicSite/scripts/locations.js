/** @format */

// JavaScript Document

// Map
var map

// Datatable
var table

var isDefaultTableSetup = false

var markers = []
var windows = []

var lastInfoWindow

var selected = []

var selectedCount

var currentCarouselID

var isSliding = false

var locs

var prevPos
var prevZoomLvl = 7
var locZoomLvl = 10
var defaultZoomLvl = 7

var justAutoZoomed = false
var previouslyTriggeredCarouselBtn = false

var ServiceColText = 'SERVICE'
var ApplyColText = 'APPLY'
var ProviderProgramColText = 'PARTNER/PROGRAM/AGENCY'
var DurationOfTrainingColText = 'DURATION OF TRAINING'
var IDColText = 'ID'
var EmployerPOCColText = 'EMPLOYER POC'
var POCEmailColText = 'POC E-MAIL'
var ClosestInstallationColText = 'CLOSEST INSTALLATION'
var StateColText = 'STATE'
var LocationsOfProspectiveJobsByStateColText = 'LOCATIONS OF PROSPECTIVE JOBS BY STATE'
var DeliveryMethodColText = 'DELIVERY METHOD'
var TargetMOCsColText = 'TARGET MOCs'
var OtherEligibilityFactorsColText = 'OTHER ELIGIBILITY FACTORS'
var OtherColText = 'OTHER'
var MOUsColText = 'MOUs'
var JobsDescriptionColText = 'JOBS DESCRIPTION'
var SummaryDescriptionColText = 'SUMMARY DESCRIPTION'
var CityColText = 'CITY'
var ZipColText = 'ZIP'
var JobFamilyColText = 'JOB FAMILY'
var CompanyColText = 'COMPANY'
var GroupIDColText = 'GROUPID'

var ServiceColIndex = -1
var ApplyColIndex = -1
var ProviderProgramColIndex = -1
var DurationOfTrainingColIndex = -1
var IDColIndex = -1
var EmployerPOCColIndex = -1
var POCEmailColIndex = -1
var ClosestInstallationColIndex = -1
var StateColIndex = -1
var LocationsOfProspectiveJobsByStateColIndex = -1
var DeliveryMethodColIndex = -1
var TargetMOCsColIndex = -1
var OtherEligibilityFactorsColIndex = -1
var OtherColIndex = -1
var MOUsColIndex = -1
var JobsDescriptionColIndex = -1
var SummaryDescriptionColIndex = -1
var CityColIndex = -1
var ZipColIndex = -1
var JobFamilyColIndex = -1
var CompanyColIndex = -1
var GroupIDColIndex = -1

var searchedProvider = ''
var searchedService = ''
var searchedDuration = ''
var searchedDelivery = ''
var searchedLocProspective = ''
var searchedJobFamily = ''
var searchedCompany = ''

var savedS3Data = null

var liveLocationsFile = 'locations.json'
var devLocationsFile = 'locationsGen.json'

var isLive = true

$(document).ready(function () {
  SetupDataTables()

  $('#search-form').submit(function (e) {
    e.preventDefault()
  })
})

// This will fire once all ajax calls are complete
$(document).ajaxStop(function () {
  if (document.location.hostname == 'skillbridge.osd.mil') {
    isLive = true
  }

  //InitDefaultMap();
  ShowLoadingScreen()
  SetupFilters()
  SetupSearch()
  HideLoadingScreen()
  EnableTooltips()
  InitializeGoogleAnalyticsEvents()
})

function SetColumnIndex(columnName, index) {
  if (columnName == ServiceColText) {
    ServiceColIndex = index
  } else if (columnName == ApplyColText) {
    ApplyColIndex = index
  } else if (columnName == ProviderProgramColText) {
    ProviderProgramColIndex = index
  } else if (columnName == DurationOfTrainingColText) {
    DurationOfTrainingColIndex = index
  } else if (columnName == IDColText) {
    IDColIndex = index
  } else if (columnName == EmployerPOCColText) {
    EmployerPOCColIndex = index
  } else if (columnName == POCEmailColText) {
    POCEmailColIndex = index
  } else if (columnName == ClosestInstallationColText) {
    ClosestInstallationColIndex = index
  } else if (columnName == StateColText) {
    StateColIndex = index
  } else if (columnName == LocationsOfProspectiveJobsByStateColText) {
    LocationsOfProspectiveJobsByStateColIndex = index
  } else if (columnName == DeliveryMethodColText) {
    DeliveryMethodColIndex = index
  } else if (columnName == TargetMOCsColText) {
    TargetMOCsColIndex = index
  } else if (columnName == OtherEligibilityFactorsColText) {
    OtherEligibilityFactorsColIndex = index
  } else if (columnName == OtherColText) {
    OtherColIndex = index
  } else if (columnName == MOUsColText) {
    MOUsColIndex = index
  } else if (columnName == JobsDescriptionColText) {
    JobsDescriptionColIndex = index
  } else if (columnName == SummaryDescriptionColText) {
    SummaryDescriptionColIndex = index
  } else if (columnName == CityColText) {
    CityColIndex = index
  } else if (columnName == ZipColText) {
    ZipColIndex = index
  } else if (columnName == JobFamilyColText) {
    JobFamilyColIndex = index
  } else if (columnName == CompanyColText) {
    CompanyColIndex = index
  } else if (columnName == GroupIDColText) {
    GroupIDColIndex = index
  }
}

function SearchOnS3() {
  var where = ''
  var program = $('#provider-dropdown').find('option:selected')[0].value != null ? $('#provider-dropdown').find('option:selected')[0].value : ''
  var service = $('#service-dropdown').find('option:selected')[0].value != null ? $('#service-dropdown').find('option:selected')[0].value : ''
  var duration = $('#duration-of-training-dropdown').find('option:selected')[0].value != null ? $('#duration-of-training-dropdown').find('option:selected')[0].value : ''
  var delivery = $('#delivery-method-dropdown').find('option:selected')[0].value != null ? $('#delivery-method-dropdown').find('option:selected')[0].value : ''
  var location = $('#location-of-prospective-jobs-dropdown').find('option:selected')[0].value != null ? $('#location-of-prospective-jobs-dropdown').find('option:selected')[0].value : ''
  var jobfamily = $('#job-family-dropdown').find('option:selected')[0].value != null ? $('#job-family-dropdown').find('option:selected')[0].value : ''
  //var company = $("#company-dropdown").find("option:selected")[0].value != null ? $("#company-dropdown").find("option:selected")[0].value : "";

  /*
        var searchedProvider = "";
        var searchedService = "";
        var searchedDuration = "";
        var searchedDelivery = "";
        var searchedLocProspective = "";
        var searchedJobFamily = "";
        var searchedCompany = "";
    */

  if (program != '-1') {
    searchedProvider = program
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where s.PROGRAM = '" + program + "'" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and s.PROGRAM = '" + program + "'" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedProvider = ''
  }

  if (service != '-1') {
    searchedService = service
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where lower(s.SERVICE) like lower('%" + service + "%')" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and lower(s.SERVICE) like lower('%" + service + "%')" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedService = ''
  }

  if (duration != '-1') {
    searchedDuration = duration
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where lower(s.DURATIONOFTRAINING) like lower('%" + duration + "%')" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and lower(s.DURATIONOFTRAINING) like lower('%" + duration + "%')" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedDuration = ''
  }

  if (delivery != '-1') {
    searchedDelivery = delivery
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where lower(s.DELIVERY_METHOD) like lower('%" + delivery + "%')" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and lower(s.DELIVERY_METHOD) like lower('%" + delivery + "%')" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedDelivery = ''
  }

  if (location != '-1') {
    searchedLocProspective = location
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where lower(s.LOCATIONSOFPROSPECTIVEJOBSBYSTATE) like lower('%" + location + "%')" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and lower(s.LOCATIONSOFPROSPECTIVEJOBSBYSTATE) like lower('%" + location + "%')" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedLocProspective = ''
  }

  if (jobfamily != '-1') {
    searchedJobFamily = jobfamily
    if (where == '') {
      //var where = "where s.onet_ids like '%," + onetId + ",%'";
      where += "where lower(s.JOBFAMILIES) like lower('%" + jobfamily + "%')" // MUST use SINGLE quotes within the SQL statement
    } else {
      where += " and lower(s.JOBFAMILIES) like lower('%" + jobfamily + "%')" // MUST use SINGLE quotes within the SQL statement
    }
  } else {
    searchedJobFamily = ''
  }

  /*if (company != "-1"){
        searchedCompany = company;
        if(where == "")
        {
            //var where = "where s.onet_ids like '%," + onetId + ",%'";
            where += "where s.ORGANIZATION like '%" + company + "%'";  // MUST use SINGLE quotes within the SQL statement
        }
        else
        {
            where += " and s.ORGANIZATION like '%" + company + "%'";  // MUST use SINGLE quotes within the SQL statement
        }
    }
    else
    {
        searchedCompany = "";
    }*/

  // keyword search sql
  var keywords = ''
  var keywordSQL = ''

  $('#keywords').each(function () {
    keywords += $('#keywords').val() + ' '
  })

  // If we have keywords defined in the keyword search, add them to the sql statement
  if (keywords != '') {
    $('#keywords').each(function () {
      var thisVal = $(this).val().toLowerCase()
      if (where != '' || keywordSQL != '') {
        keywordSQL += " and (lower(s.SERVICE) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.PROGRAM) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.DURATIONOFTRAINING) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.EMPLOYERPOC) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.EMPLOYERPOCEMAIL) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.INSTALLATION) like lower('%" + thisVal + "%')"
        //keywordSQL += " or lower(s.STATE) like '%" + thisVal + "%'";
        keywordSQL += " or lower(s.LOCATIONSOFPROSPECTIVEJOBSBYSTATE) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.DELIVERY_METHOD) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.TARGETMOCs) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.OTHERELIGIBILITYFACTORS) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.OTHER) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.MOUs) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.JOBSDESCRIPTION) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.SUMMARYDESCRIPTION) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.JOBFAMILIES) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.ORGANIZATION) like lower('%" + thisVal + "%'))"
        //keywordSQL += " or lower(s.CITY) like '%" + thisVal + "%'";
        //keywordSQL += " or lower(s.ZIP) like '%" + thisVal + "%')";
      } else {
        keywordSQL += "where (lower(s.SERVICE) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.PROGRAM) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.DURATIONOFTRAINING) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.EMPLOYERPOC) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.EMPLOYERPOCEMAIL) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.INSTALLATION) like lower('%" + thisVal + "%')"
        //keywordSQL += " or lower(s.STATE) like '%" + thisVal + "%'";
        keywordSQL += " or lower(s.LOCATIONSOFPROSPECTIVEJOBSBYSTATE) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.DELIVERY_METHOD) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.TARGETMOCs) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.OTHERELIGIBILITYFACTORS) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.OTHER) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.MOUs) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.JOBSDESCRIPTION) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.SUMMARYDESCRIPTION) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.JOBFAMILIES) like lower('%" + thisVal + "%')"
        keywordSQL += " or lower(s.ORGANIZATION) like lower('%" + thisVal + "%'))"
        //keywordSQL += " or lower(s.CITY) like '%" + thisVal + "%'";
        //keywordSQL += " or lower(s.ZIP) like '%" + thisVal + "%')";
      }
    })
  }

  where += keywordSQL

  var locationSearch = $('#location').val()
  var locationSQL = ''

  if (locationSearch != '') {
    var capitalizedName = capitalizeFirstLetter(locationSearch)
    //var lowerRegex = "AL|Alabama|AK|Alaska|AZ|Arizona|AR|Arkansas|CA|California|CO|Colorado|CT|Connecticut|DE|Delaware|FL|Florida|GA|Georgia|HI|Hawaii|ID|Idaho|IL|Illinois|IN|Indiana|IA|Iowa|KS|Kansas|KY|Kentucky|LA|Louisiana|ME|Maine|MD|Maryland|MA|Massachusetts|MI|Michigan|MN|Minnesota|MS|Mississippi|MO|Missouri|MT|Montana|NE|Nebraska|NV|Nevada|NH|New Hampshire|NJ|New Jersey|NM|New Mexico|NY|New York|NC|North Carolina|ND|North Dakota|OH|Ohio|OK|Oklahoma|OR|Oregon|PA|Pennsylvania|RI|Rhode Island|SC|South Carolina|SD|South Dakota|TN|Tennessee|TX|Texas|UT|Utah|VT|Vermont|VA|Virginia|WA|Washington|WV|West Virginia|WI|Wisconsin|WY|Wyoming".toLowerCase();
    //console.log(lowerRegex);
    // Figure out what was entered
    if (/\d{5}/.test(locationSearch)) {
      // Zip code?
      //console.log("Its a zip code");
      if (where != '' || locationSQL != '') {
        locationSQL += " and (lower(s.ZIP) like lower('%" + locationSearch + "%'))"
      } else {
        locationSQL += "where (lower(s.ZIP) like lower('%" + locationSearch + "%'))"
      }
      //table.column(ZipColIndex).search( locationSearch, true, false, false).draw();
    }
    //else if(/(A[KLRZ]|C[AOT]|D[CE]|FL|GA|HI|I[ADLN]|K[SY]|LA|M[ADEINOST]|N[CDEHJMVY]|O[HKR]|PA|RI|S[CD]|T[NX]|UT|V[AT]|W[AIVY])/.test(locationSearch.toUpperCase()) && /\d{2}/.test(locationSearch))   // State abbreviation?
    //else if(/AL|Alabama|AK|Alaska|AZ|Arizona|AR|Arkansas|CA|California|CO|Colorado|CT|Connecticut|DE|Delaware|FL|Florida|GA|Georgia|HI|Hawaii|ID|Idaho|IL|Illinois|IN|Indiana|IA|Iowa|KS|Kansas|KY|Kentucky|LA|Louisiana|ME|Maine|MD|Maryland|MA|Massachusetts|MI|Michigan|MN|Minnesota|MS|Mississippi|MO|Missouri|MT|Montana|NE|Nebraska|NV|Nevada|NH|New Hampshire|NJ|New Jersey|NM|New Mexico|NY|New York|NC|North Carolina|ND|North Dakota|OH|Ohio|OK|Oklahoma|OR|Oregon|PA|Pennsylvania|RI|Rhode Island|SC|South Carolina|SD|South Dakota|TN|Tennessee|TX|Texas|UT|Utah|VT|Vermont|VA|Virginia|WA|Washington|WV|West Virginia|WI|Wisconsin|WY|Wyoming/.test(locationSearch))
    //else if(/al|alabama|ak|alaska|az|arizona|ar|arkansas|ca|california|co|colorado|ct|connecticut|de|delaware|fl|florida|ga|georgia|hi|hawaii|id|idaho|il|illinois|in|indiana|ia|iowa|ks|kansas|ky|kentucky|la|louisiana|me|maine|md|maryland|ma|massachusetts|mi|michigan|mn|minnesota|ms|mississippi|mo|missouri|mt|montana|ne|nebraska|nv|nevada|nh|new hampshire|nj|new jersey|nm|new mexico|ny|new york|nc|north carolina|nd|north dakota|oh|ohio|ok|oklahoma|or|oregon|pa|pennsylvania|ri|rhode island|sc|south carolina|sd|south dakota|tn|tennessee|tx|texas|ut|utah|vt|vermont|va|virginia|wa|washington|wv|west virginia|wi|wisconsin|wy|wyoming/.test(locationSearch.toLowerCase()))
    else if (locationSearch.length == 2 && /AL|AK|AZ|AR|CA|CO|CT|DE|FL|GA|HI|ID|IL|IN|IA|KS|KY|LA|ME|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|OH|OK|OR|PA|RI|SC|SD|TN|TX|UT|VT|VA|WA|WV|WI|WY/.test(locationSearch.toUpperCase())) {
      //console.log("Its a state abbreviation");
      if (where != '' || locationSQL != '') {
        locationSQL += " and (lower(s.STATE) like lower('%" + locationSearch.toLowerCase() + "%'))"
      } else {
        locationSQL += "where (lower(s.STATE) like lower('%" + locationSearch.toLowerCase() + "%'))"
      }
      //table.column(StateColIndex).search( locationSearch + "| Nationwide (All States)", true, false, false).draw();
    } else if (DoesStringMatchStateName(locationSearch)) {
      //console.log("Its a state name");
      var abbr = abbrState(locationSearch, 'abbr')
      abbr = abbr.toLowerCase()
      if (where != '' || locationSQL != '') {
        locationSQL += " and (lower(s.STATE) like lower('%" + abbr + "%'))"
      } else {
        locationSQL += "where (lower(s.STATE) like lower('%" + abbr + "%'))"
      }
    }
    else if(/alabama|alaska|arizona|arkansas|california|colorado|connecticut|delaware|florida|georgia|hawaii|idaho|illinois|indiana|iowa|kansas|kentucky|louisiana|maine|maryland|massachusetts|michigan|minnesota|mississippi|missouri|montana|nebraska|nevada|new\shampshire|new\sjersey|new\smexico|new\syork|north\scarolina|north\sdakota|ohio|oklahoma|oregon|pennsylvania|rhode\sisland|south\scarolina|south\sdakota|tennessee|texas|utah|vermont|virginia|washington|west\svirginia|wisconsin|wyoming/.test(locationSearch.toLowerCase()))
      {
          //console.log("Its a full state name OR city name");
          var abbr = abbrState(locationSearch, "abbr");
          abbr = abbr.toLowerCase();
          if(where != "" || locationSQL != "")
          {
              locationSQL += " and (lower(s.STATE) like lower('%" + abbr + "%'))";
              locationSQL += " or (lower(s.CITY) like lower('%" + locationSearch + "%'))";
          }
          else
          {
              locationSQL += "where (lower(s.CITY) like lower('%" + locationSearch + "%'))";
              locationSQL += " or (lower(s.STATE) like lower('%" + abbr + "%'))";
          }
      }
    /*else if(/Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|Florida|Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|New\sHampshire|New\sJersey|New\sMexico|New\sYork|North\sCarolina|North\sDakota|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode\sIsland|South\sCarolina|South\sDakota|Tennessee|Texas|Utah|Vermont|Virginia|Washington|West\sVirginia|Wisconsin|Wyoming/.test(capitalizedName))
    {
        console.log("Its a state name");
        var abbr = abbrState(locationSearch, "abbr");
        abbr = abbr.toLowerCase();
        if(where != "" || locationSQL != "")
        {
            locationSQL += " and (lower(s.STATE) like '%" + abbr + "%')";
        }
        else
        {
            locationSQL += "where (lower(s.STATE) like '%" + abbr + "%')";
        }
    }*/ // City?
    else {
      //console.log("Its a city");
      if (where != '' || locationSQL != '') {
        locationSQL += " and (lower(s.CITY) like lower('%" + locationSearch + "%'))"
      } else {
        locationSQL += "where (lower(s.CITY) like lower('%" + locationSearch + "%'))"
      }
      //table.column(CityColIndex).search( locationSearch, true, false, false).draw();
    }

    //table.column(ZipColIndex).search( locationSearch, true, false, false).draw();
    //table.column(StateColIndex).search( locationSearch + "|Nationwide (All States)", true, false, false).draw();
    //table.column(CityColIndex).search( locationSearch, true, false, false).draw();
    //table.column(ZipColIndex).search( locationSearch, true, false, false).draw();
    //table.columns(CityColIndex, StateColIndex, ZipColIndex).search( locationSearch + "|Nationwide (All States)", true, false, false).draw();
    //table.column(CityColIndex).search( locationSearch, true, false, false).draw();

    // Filter the right column based on the determined input
  }

  where += locationSQL

  let e = 'select * from s3object[*].locations [*] s ' + where //  let e = "select * from s3object[*].moc_info[*] s " + where;
  //let e = "select * from s3object[*].locations.data [*] s " + where;  //  let e = "select * from s3object[*].moc_info[*] s " + where;
  /*var t = document.getElementById("query-text").value;
        t.length > 5 && (e += " where " + t),*/
  //console.log(e);
  //_gaq.push(['_trackEvent','Related Civilian Credentials Search','click','Search Clicked - ' + e.toString(),,true]);
  let o = {
    Bucket: 'sb-data-solid', // make parameter
    Key: isLive ? liveLocationsFile : devLocationsFile, // make parameter        // moc_info.json
    Expression: e,
    ExpressionType: 'SQL',
    InputSerialization: {
      JSON: {
        Type: 'DOCUMENT',
      },
    },
    OutputSerialization: {
      JSON: {
        RecordDelimiter: ',',
      },
    },
    RequestProgress: {
      Enabled: !0,
    },
  }
  var n = ''
  s3.selectObjectContent(o, function (e, t) {
    if (e) console.log('error'), console.log(e, e.stack)
    else {
      var events = t.Payload
      //for (var s of o) {
      // refactored for ie 11
      for (var eve in events) {
        var s = events[eve]
        if (s.Records) {
          var r = s.Records.Payload
          i = new TextDecoder('utf-8').decode(r)
          n += i
        } else if (s.Stats);
        else if (s.Progress);
        else if (s.Cont);
        else if (s.End) {
          var a = '[' + n.slice(0, n.length - 1) + ']'
          c = JSON.parse(a)
          // console.log("c",c);
          //UpdateRelatedCivilianOccupationsTableData(c);
          //UpdateRelatedCivlianCredentialTableData(c);

          //console.log("returned data: " + c);
          //console.log("first object: " + c[0]);
          //console.log("first object id: " + c[0].ID);
          ShowResultsContainer()
          UpdateTableData(c)
          AddDataToDropdowns(c)
          SelectSavedDropdownValues()
          HideLoadingScreen()
        }
      }
    }
  })
}

function DoesStringMatchStateName(val) {
  var states = ['Alabama', 'Alaska', 'American Samoa', 'Arizona', 'Arkansas', 'California', 'Colorado', 'Connecticut', 'Delaware', 'District of Columbia', 'Federated States of Micronesia', 'Florida', 'Georgia', 'Guam', 'Hawaii', 'Idaho', 'Illinois', 'Indiana', 'Iowa', 'Kansas', 'Kentucky', 'Louisiana', 'Maine', 'Marshall Islands', 'Maryland', 'Massachusetts', 'Michigan', 'Minnesota', 'Mississippi', 'Missouri', 'Montana', 'Nebraska', 'Nevada', 'New Hampshire', 'New Jersey', 'New Mexico', 'New York', 'North Carolina', 'North Dakota', 'Northern Mariana Islands', 'Ohio', 'Oklahoma', 'Oregon', 'Palau', 'Pennsylvania', 'Puerto Rico', 'Rhode Island', 'South Carolina', 'South Dakota', 'Tennessee', 'Texas', 'Utah', 'Vermont', 'Virgin Island', 'Virginia', 'Washington', 'West Virginia', 'Wisconsin', 'Wyoming']
  for (var i = 0; i < states.length; i++) {
    if (states[i].localeCompare(val) == 0) {
      return true
    }
  }

  return false
}

function UpdateTableData(e) {
  var tabledata = e.map(function (r) {
    var info = {
      ACTIONS: "<button class='table-map-location' onclick='ShowPin(" + r.LAT + ',' + r.LONG + ',"' + r.CITY.replace(/,/g, ',') + '","' + r.STATE + '","' + r.ZIP + '","' + r.PROGRAM + '","' + r.EMPLOYERPOC + '","' + r.EMPLOYERPOCEMAIL + "\")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><img src='images/icon-map-location.png' alt='View Opportunity on Map'/></button>",
      //ACTIONS: "<a href='how-to-apply.htm' class='table-how-to-apply' style='color:#355e93;text-decoration:none !important;' data-service='" + r.SERVICE + "' data-installation='" + r.INSTALLATION + "' data-program='" + r.PROGRAM + "' title='How to Apply'><img src='images/icon-apply.png' alt='How to Apply'/></a><button class='table-map-location' onclick='ShowPin(" + r.LAT + ',' + r.LONG + ',"' + r.CITY.replace(/,/g, ',') + '","' + r.STATE + '","' + r.ZIP + '","' + r.PROGRAM + '","' + r.EMPLOYERPOC + '","' + r.EMPLOYERPOCEMAIL + "\")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><img src='images/icon-map-location.png' alt='View Opportunity on Map'/></button>",
      //APPLY: "<a href='how-to-apply.htm' style='color:#355e93;text-decoration:none !important;' data-service='" + r.SERVICE +  "' data-installation='" + r.INSTALLATION +  "' data-program='" + r.PROGRAM + "'><i class='fa-solid fa-clipboard-check' style='font-size: 18px;'></i></a>",
      //VIEWONMAP: "<button onclick='ShowPin(" + r.LAT + "," + r.LONG + ")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><i class='fa-solid fa-location-dot' style='font-size: 18px;'></i></button>",
      //APPLY: "<a href='how-to-apply.htm' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + r.SERVICE +  "' data-installation='" + r.INSTALLATION +  "' data-program='" + r.PROGRAM + "'>Apply</a>",
      //VIEWONMAP:  "<button onclick='ShowPin(" + r.LAT + "," + r.LONG + ")' class='btn-secondary btn apply-btn' style='color:#ffffff;text-decoration:none !important;font-weight:bold;' data-toggle='modal' data-target='#map-dialog'>View</button>",
      ORGANIZATION: r.ORGANIZATION,
      PROGRAM: r.PROGRAM + (r.NATIONWIDE == 1 ? " <img src='images/icon-nationwide.png' class='nat' alt='Nationwide / Online' /><span class='table-hidden-data'>Nationwide</span>" : ''),
      SERVICE: r.SERVICE,
      CITY: r.CITY,
      STATE: r.STATE,
      ZIP: r.ZIP,
      DURATIONOFTRAINING: r.DURATIONOFTRAINING,
      EMPLOYERPOC: r.EMPLOYERPOC,
      EMPLOYERPOCEMAIL: linkify(r.EMPLOYERPOCEMAIL),
      COST:adjustCost(r.COST),
      INSTALLATION: r.INSTALLATION,
      LOCATIONSOFPROSPECTIVEJOBSBYSTATE: linkify(r.LOCATIONSOFPROSPECTIVEJOBSBYSTATE),
      DELIVERY_METHOD: r.DELIVERY_METHOD,
      TARGETMOCs: r.TARGETMOCs,
      OTHERELIGIBILITYFACTORS: linkify(r.OTHERELIGIBILITYFACTORS),
      OTHER: linkify(r.OTHER),
      //MOUs: r.MOUs,
      JOBSDESCRIPTION: linkify(r.JOBSDESCRIPTION),
      SUMMARYDESCRIPTION: linkify(r.SUMMARYDESCRIPTION),
      JOBFAMILIES: r.JOBFAMILIES,
      PARENTORGANIZATION: r.PARENTORGANIZATION,
    }
    return info
  })

  var data = []
  data = json2arraynokeys(tabledata) // convert json to array with no keys so data can be added to table
  table.clear() // remove any existing data
  table.rows.add(data).draw() // add data and redraw
  table.responsive.recalc()
}

function ShowPin(lat, long, city, state, zip, program, poc, pocemail) {
  ShowMapContainer()
  var singleLocationIcon = 'images/icon-map-location.png'

  map = new google.maps.Map(document.getElementById('map-container'), {
    center: { lat: lat, lng: long },
    height: 300,
    zoom: defaultZoomLvl,
    fullscreenControl: false,
  })

  // The marker
  const marker = new google.maps.Marker({
    position: { lat: lat, lng: long },
    map: map,
    icon: singleLocationIcon,
  })

  // Add the information window popup for the marker
  //AddInfoWindow(marker, currentLocation, 1, installationItems);

  var message = "<div style='padding:0.75em 0.25em;width:100%;line-height:28px;font-size:16px;'>"
  message += "<span style='font-weight:bold;'>" + program + '</span><br/>'
  message += '<span>' + city + ', ' + state + ' ' + zip + '</span><br/>'
  message += '<span>' + poc + '</span>&nbsp;('
  message += "<span><a href='mailto:" + pocemail + "' target='_blank'>" + pocemail + '</a></span>)'
  message += '</div>'

  var infoWindow = new google.maps.InfoWindow({
    content: message,
    width: 320,
    title: program,
  })

  infoWindow.open(map, marker)
}

function SetupFilters() {
  //PopulateDropdowns(locations.data);

  //PopulateDropdownsFromS3();

  PopulateDropdownsFromOptimizedData()

  $('.select2-dropdown').select2({
    dropdownAutoWidth: true,
    width: '100%',
  })
}

function SetupSearch() {
  $('#loc-search-btn')
    .unbind()
    .click(function () {
      // Get values for all filters
      //BuildFilters();
      //console.log("search clicked");
      ShowLoadingScreen()
      SearchOnS3()

      // Search on specific columns first, then use the generic search for keywords and locations

      //$("#location-table").column(10).search(this.value).draw();
      //table.column(ProviderProgramColIndex).search("7 Eagle Group").draw();
      //table.column(DurationOfTrainingColIndex).search("151 - 180 days").draw();
    })

  $('#reset-btn')
    .unbind()
    .click(function () {
      searchedProvider = ''
      searchedService = ''
      searchedDuration = ''
      searchedDelivery = ''
      searchedLocProspective = ''
      searchedJobFamily = ''
      searchedCompany = ''
      $('#keywords').val('')
      $('#location').val('')
      HideResultsContainer()
      ShowLoadingScreen()
      //PopulateDropdownsFromS3();
      PopulateDropdownsFromOptimizedData()
      /*if(savedS3Data != null)
        {
            AddDataToDropdowns(savedS3Data);
        }*/

      //SelectDefaultDropdownVals();
      HideLoadingScreen()

      _gaq.push(['_trackEvent', 'Locations Map', 'click', 'Reset Clicked', , true])
      gtag('event', 'Reset Clicked', {
        event_category: 'Locations Map',
        page_url: window.location.href
      });
    })

  $('.select2-dropdown')
    .unbind()
    .change(function () {
      ShowLoadingScreen()
      SearchOnS3()

      var id = $(this).attr('id')
      var val = $(this).val();
      var text = $(this).find(':selected').text();
      //_gaq.push(['_trackEvent', 'Locations Map', 'change', 'Dropdown Changed (' + id + ') - ' + val, , true])
      gtag('event', 'Dropdown Changed (' + id + ')', {
        event_category: 'Locations Map',
        value: val,
        text: text,
        page_url: window.location.href
      });
    })
}

function PopulateDropdowns(data) {
  /* PROGRAMS/PROVIDERS */
  // Get Unique Programs
  var uniqueProgramsItems = _.uniq(data, (item) => item.PROGRAM)
  //console.log("uniqueProgramsItems.length: " + uniqueProgramsItems.length);
  var uniquePrograms = new Array()

  for (var i = 0; i < uniqueProgramsItems.length; i++) {
    uniquePrograms.push(uniqueProgramsItems[i].PROGRAM)
    //console.log("uniqueProgramsItems.PROGRAM: " + uniqueProgramsItems[i].PROGRAM);
  }

  //console.log("====uniquePrograms=====: " + uniquePrograms);

  // Sort Alphebetically
  uniquePrograms.sort(function (a, b) {
    return a.toLowerCase().localeCompare(b.toLowerCase())
  })

  var uniqueProgramsForExport = 'const programs = new Array('

  for (var i = 0; i < uniquePrograms.length; i++) {
    if (i == 0) {
      uniqueProgramsForExport += '"' + uniquePrograms[i] + '"'
    } else {
      uniqueProgramsForExport += ', "' + uniquePrograms[i] + '"'
    }
  }

  uniqueProgramsForExport += ');'
  //console.log("uniqueProgramsForExport: " + uniqueProgramsForExport);

  // Create dropdown HTML
  var newProviderHTML = "<option value='-1'>Partner</option>"

  for (var x = 0; x < uniquePrograms.length; x++) {
    newProviderHTML += "<option value='" + uniquePrograms[x] + "'>" + uniquePrograms[x] + '</option>'
  }

  // Add to dropdown
  $('#provider-dropdown').html(newProviderHTML)

  /* SERVICES */
  // Get Unique Services
  var uniqueServicesItems = _.uniq(data, (item) => item.SERVICE)
  var servicesList = new Array()
  var uniqueServices = new Array()

  for (var i = 0; i < uniqueServicesItems.length; i++) {
    var splits = uniqueServicesItems[i].SERVICE.split(',')

    for (var x = 0; x < splits.length; x++) {
      var item = splits[x].trim()
      if (item != '' && item != ' ' && item != 'All Services') {
        servicesList.push(item)
      }
    }
  }

  uniqueServices = [...new Set(servicesList)]

  // Sort Alphebetically
  uniqueServices.sort()

  var uniqueServicesForExport = 'const services = new Array('

  for (var i = 0; i < uniqueServices.length; i++) {
    if (i == 0) {
      uniqueServicesForExport += '"' + uniqueServices[i] + '"'
    } else {
      uniqueServicesForExport += ', "' + uniqueServices[i] + '"'
    }
  }

  uniqueServicesForExport += ');'
  //console.log("uniqueServicesForExport: " + uniqueServicesForExport);

  // Create dropdown HTML
  var newServicesHTML = "<option value='-1'>All Services</option>"

  for (var x = 0; x < uniqueServices.length; x++) {
    newServicesHTML += "<option value='" + uniqueServices[x] + "'>" + uniqueServices[x] + '</option>'
  }

  // Add to dropdown
  $('#service-dropdown').html(newServicesHTML)

  /* DURATION OF TRAINING */
  // Get Unique Durations
  var uniqueDurationItems = _.uniq(data, (item) => item.DURATIONOFTRAINING)
  var durationsList = new Array()
  var uniqueDurations = new Array()

  for (var i = 0; i < uniqueDurationItems.length; i++) {
    var splits = uniqueDurationItems[i].DURATIONOFTRAINING.split(',')

    for (var x = 0; x < splits.length; x++) {
      var item = splits[x].trim()
      if (item != '' && item != ' ') {
        durationsList.push(item)
      }
    }
  }

  uniqueDurations = [...new Set(durationsList)]

  // Sort Alphebetically
  var collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' })
  uniqueDurations.sort(collator.compare)

  var uniqueDurationsForExport = 'const durations = new Array('

  for (var i = 0; i < uniqueDurations.length; i++) {
    if (i == 0) {
      uniqueDurationsForExport += '"' + uniqueDurations[i] + '"'
    } else {
      uniqueDurationsForExport += ', "' + uniqueDurations[i] + '"'
    }
  }

  uniqueDurationsForExport += ');'
  //console.log("uniqueDurationsForExport: " + uniqueDurationsForExport);

  // Create dropdown HTML
  var newDurationHTML = "<option value='-1'>Duration of Training</option>"

  for (var x = 0; x < uniqueDurations.length; x++) {
    newDurationHTML += "<option value='" + uniqueDurations[x] + "'>" + uniqueDurations[x] + '</option>'
  }

  // Add to dropdown
  $('#duration-of-training-dropdown').html(newDurationHTML)

  /* DELIVERY METHOD */
  // Get Unique Delivery Methods
  var uniqueDeliveryItems = _.uniq(data, (item) => item.DELIVERY_METHOD)
  var uniqueDelivery = new Array()

  for (var i = 0; i < uniqueDeliveryItems.length; i++) {
    var item = uniqueDeliveryItems[i].DELIVERY_METHOD.trim()
    if (item != '' && item != ' ') {
      uniqueDelivery.push(uniqueDeliveryItems[i].DELIVERY_METHOD)
    }
  }

  // Sort Alphebetically
  uniqueDelivery.sort()

  var uniqueDeliveryForExport = 'const deliveries = new Array('

  for (var i = 0; i < uniqueDelivery.length; i++) {
    if (i == 0) {
      uniqueDeliveryForExport += '"' + uniqueDelivery[i] + '"'
    } else {
      uniqueDeliveryForExport += ', "' + uniqueDelivery[i] + '"'
    }
  }

  uniqueDeliveryForExport += ');'
  //console.log("uniqueDeliveryForExport: " + uniqueDeliveryForExport);

  // Create dropdown HTML
  var newDeliveryHTML = "<option value='-1'>Delivery Method</option>"

  for (var x = 0; x < uniqueDelivery.length; x++) {
    newDeliveryHTML += "<option value='" + uniqueDelivery[x] + "'>" + uniqueDelivery[x] + '</option>'
  }

  // Add to dropdown
  $('#delivery-method-dropdown').html(newDeliveryHTML)

  /* LOCATION OF PROSPECTIVE JOBS */
  // Get Unique Locations
  var uniqueLocationItems = _.uniq(data, (item) => item.LOCATIONSOFPROSPECTIVEJOBSBYSTATE)
  var locationsList = new Array()
  var uniqueLocations = new Array()

  for (var i = 0; i < uniqueLocationItems.length; i++) {
    var splits = uniqueLocationItems[i].LOCATIONSOFPROSPECTIVEJOBSBYSTATE.split(',')

    for (var x = 0; x < splits.length; x++) {
      var item = splits[x].trim()
      if (item != '' && item != ' ' && item != 'All Services') {
        locationsList.push(item)
      }
    }
  }

  uniqueLocations = [...new Set(locationsList)]

  // Sort Alphebetically
  uniqueLocations.sort()

  var uniqueLocationsForExport = 'const locations = new Array('

  for (var i = 0; i < uniqueLocations.length; i++) {
    if (i == 0) {
      uniqueLocationsForExport += '"' + uniqueLocations[i] + '"'
    } else {
      uniqueLocationsForExport += ', "' + uniqueLocations[i] + '"'
    }
  }

  uniqueLocationsForExport += ');'
  //console.log("uniqueLocationsForExport: " + uniqueLocationsForExport);

  // Create dropdown HTML
  var newLocationHTML = "<option value='-1'>Location of Prospective Jobs</option>"

  for (var x = 0; x < uniqueLocations.length; x++) {
    newLocationHTML += "<option value='" + uniqueLocations[x] + "'>" + uniqueLocations[x] + '</option>'
  }

  // Add to dropdown
  $('#location-of-prospective-jobs-dropdown').html(newLocationHTML)

  /* JOB FAMILY */
  // Get Unique Job Families
  var uniqueFamilyItems = _.uniq(data, (item) => item.JOBFAMILIES)
  var familiesList = new Array()
  var uniqueFamilies = new Array()

  for (var i = 0; i < uniqueFamilyItems.length; i++) {
    //if(uniqueFamilyItems[i].JOBFAMILIES != undefined)
    //{
    //console.log("JOBFAMILIES FOR ITEM: " + uniqueFamilyItems[i].JOBFAMILIES);
    //console.log("ORGANIZATION FOR ITEM: " + uniqueFamilyItems[i].ORGANIZATION);
    var splits = uniqueFamilyItems[i].JOBFAMILIES.split(';')

    for (var x = 0; x < splits.length; x++) {
      var item = splits[x].trim()
      if (item != '' && item != ' ') {
        familiesList.push(item)
      }
    }
    //}
  }

  uniqueFamilies = [...new Set(familiesList)]

  // Sort Alphebetically
  uniqueFamilies.sort()

  var uniqueFamiliesForExport = 'const families = new Array('

  for (var i = 0; i < uniqueFamilies.length; i++) {
    if (i == 0) {
      uniqueFamiliesForExport += '"' + uniqueFamilies[i] + '"'
    } else {
      uniqueFamiliesForExport += ', "' + uniqueFamilies[i] + '"'
    }
  }

  uniqueFamiliesForExport += ');'
  //console.log("uniqueFamiliesForExport: " + uniqueFamiliesForExport);

  // Create dropdown HTML
  var newFamilyHTML = "<option value='-1'>Industries</option>"

  for (var x = 0; x < uniqueFamilies.length; x++) {
    newFamilyHTML += "<option value='" + uniqueFamilies[x] + "'>" + uniqueFamilies[x] + '</option>'
  }

  // Add to dropdown
  $('#job-family-dropdown').html(newFamilyHTML)

  /* COMPANY */
  // Get Unique Companies
  var uniqueParentOrgItems = _.uniq(data, (item) => item.PARENTORGANIZATION)
  //console.log("uniqueParentOrgItems.length: " + uniqueParentOrgItems.length);
  var uniqueParentOrgs = new Array()

  for (var i = 0; i < uniqueParentOrgItems.length; i++) {
    //console.log("uniqueParentOrgItems[i].ORGANIZATION: " + uniqueParentOrgItems[i].PARENTORGANIZATION);
    uniqueParentOrgs.push(uniqueParentOrgItems[i].PARENTORGANIZATION)
  }

  uniqueParentOrgs = [...new Set(uniqueParentOrgs)]

  // Sort Alphebetically
  uniqueParentOrgs.sort()

  var uniqueParentOrgsForExport = 'const parentOrgs = new Array('

  for (var i = 0; i < uniqueParentOrgs.length; i++) {
    if (i == 0) {
      uniqueParentOrgsForExport += '"' + uniqueParentOrgs[i] + '"'
    } else {
      uniqueParentOrgsForExport += ', "' + uniqueParentOrgs[i] + '"'
    }
  }

  uniqueParentOrgsForExport += ');'
  //console.log("uniqueParentOrgsForExport: " + uniqueParentOrgsForExport);

  // Create dropdown HTML
  var newOrganizationHTML = "<option value='-1'>Company</option>"

  var relatedOrgsForExport = 'var relatedOrgs = { data: ['

  // Find all Orgs under each parent org
  for (var i = 0; i < uniqueParentOrgs.length; i++) {
    var orgItems = _.where(data, { PARENTORGANIZATION: uniqueParentOrgs[i] })
    var uniqueOrgItems = _.uniq(orgItems, (item) => item.ORGANIZATION)
    uniqueOrgItems = [...new Set(uniqueOrgItems)]
    uniqueOrgItems.sort()

    // If multiple orgs under parent org, use opt group
    if (uniqueOrgItems.length > 1) {
      if (i > 0) {
        relatedOrgsForExport += ','
      }
      relatedOrgsForExport += '{ "parentOrg": "' + uniqueParentOrgs[i] + '","orgs":['
      newOrganizationHTML += '<optgroup label="' + uniqueParentOrgs[i] + '">'

      for (var j = 0; j < uniqueOrgItems.length; j++) {
        newOrganizationHTML += "<option value='" + uniqueOrgItems[j].ORGANIZATION + "'>" + uniqueOrgItems[j].ORGANIZATION + '</option>'
        if (j > 0) {
          relatedOrgsForExport += ','
        }
        relatedOrgsForExport += '"' + uniqueOrgItems[j].ORGANIZATION + '"'
      }

      relatedOrgsForExport += ']}'
      newOrganizationHTML += '</optgroup>'
    } else {
      if (i > 0) {
        relatedOrgsForExport += ','
      }
      relatedOrgsForExport += '{ "parentOrg": "' + uniqueParentOrgs[i] + '","orgs":['
      for (var j = 0; j < uniqueOrgItems.length; j++) {
        newOrganizationHTML += "<option value='" + uniqueOrgItems[j].ORGANIZATION + "'>" + uniqueOrgItems[j].ORGANIZATION + '</option>'
        if (j > 0) {
          relatedOrgsForExport += ','
        }
        relatedOrgsForExport += '"' + uniqueOrgItems[j].ORGANIZATION + '"'
      }
      relatedOrgsForExport += ']}'
    }
  }

  relatedOrgsForExport += ']}'

  //console.log("relatedOrgsForExport: " + relatedOrgsForExport);

  /*
        <optgroup label="Group Name">
            <option>Nested option</option>
        </optgroup>
    */

  /*for(var x=0; x < uniqueParentOrgs.length; x++)
    {
        newOrganizationHTML += "<option value='" + uniqueParentOrgs[x] + "'>" + uniqueParentOrgs[x] + "</option>";
    }*/

  // Add to dropdown
  //$("#company-dropdown").html(newOrganizationHTML);

  /* COMPANY */
  // Get Unique Companies
  /*var uniqueCompanyItems = _.uniq(data, item => item.ORGANIZATION);
    console.log("uniqueCompanyItems.length: " + uniqueCompanyItems.length);
    var uniqueCompanys = new Array();

    for(var i=0; i < uniqueCompanyItems.length; i++)
    {
        console.log("uniqueCompanyItems[i].ORGANIZATION: " + uniqueCompanyItems[i].ORGANIZATION);
        uniqueCompanys.push(uniqueCompanyItems[i].ORGANIZATION);
    }

    uniqueCompanys = [...new Set(uniqueCompanys)];

    // Sort Alphebetically
    uniqueCompanys.sort();

    // Create dropdown HTML
    var newCompanyHTML = "<option value='-1'>Company</option>";


        <optgroup label="Group Name">
            <option>Nested option</option>
        </optgroup>


    for(var x=0; x < uniqueCompanys.length; x++)
    {
        newCompanyHTML += "<option value='" + uniqueCompanys[x] + "'>" + uniqueCompanys[x] + "</option>"
    }

    // Add to dropdown
    $("#company-dropdown").html(newCompanyHTML);*/
}

function PopulateDropdownsFromOptimizedData() {
  /* PROGRAMS/PROVIDERS */
  // Create dropdown HTML
  var newProviderHTML = "<option value='-1'>Partner</option>"
  //var testouput;

  // Sort just in case it's out of order from the data
  programDropdown.sort(function (a, b) {
    return a.toLowerCase().localeCompare(b.toLowerCase())
  })

  for (var x = 0; x < programDropdown.length; x++) {
    newProviderHTML += "<option value='" + programDropdown[x] + "'>" + programDropdown[x] + '</option>'

    /*if(x == 0)
        {
            testouput += programDropdown[x];
        }
        else
        {
            testouput += ", " + programDropdown[x];
        }*/
  }

  //console.log(testouput);

  // Add to dropdown
  $('#provider-dropdown').html(newProviderHTML)

  /* SERVICES */

  // Create dropdown HTML
  var newServicesHTML = "<option value='-1'>All Services</option>"

  for (var x = 0; x < serviceDropdown.length; x++) {
    newServicesHTML += "<option value='" + serviceDropdown[x] + "'>" + serviceDropdown[x] + '</option>'
  }

  // Add to dropdown
  $('#service-dropdown').html(newServicesHTML)

  /* DURATION OF TRAINING */
  // Create dropdown HTML
  var newDurationHTML = "<option value='-1'>Duration of Training</option>"

  // Sort Alphebetically In Case it's not already
  var collator = new Intl.Collator(undefined, { numeric: true, sensitivity: 'base' })
  durationDropdown.sort(collator.compare)

  for (var x = 0; x < durationDropdown.length; x++) {
    newDurationHTML += "<option value='" + durationDropdown[x] + "'>" + durationDropdown[x] + '</option>'
  }

  // Add to dropdown
  $('#duration-of-training-dropdown').html(newDurationHTML)

  /* DELIVERY METHOD */
  // Create dropdown HTML
  var newDeliveryHTML = "<option value='-1'>Delivery Method</option>"

  for (var x = 0; x < deliveryDropdown.length; x++) {
    newDeliveryHTML += "<option value='" + deliveryDropdown[x] + "'>" + deliveryDropdown[x] + '</option>'
  }

  // Add to dropdown
  $('#delivery-method-dropdown').html(newDeliveryHTML)

  /* LOCATION OF PROSPECTIVE JOBS */
  // Create dropdown HTML
  var newLocationHTML = "<option value='-1'>Location of Prospective Jobs</option>"

  for (var x = 0; x < locationDropdown.length; x++) {
    newLocationHTML += "<option value='" + locationDropdown[x] + "'>" + locationDropdown[x] + '</option>'
  }

  // Add to dropdown
  $('#location-of-prospective-jobs-dropdown').html(newLocationHTML)

  /* JOB FAMILY */
  // Create dropdown HTML
  var newFamilyHTML = "<option value='-1'>Industries</option>"

  for (var x = 0; x < familyDropdown.length; x++) {
    newFamilyHTML += "<option value='" + familyDropdown[x] + "'>" + familyDropdown[x] + '</option>'
  }

  // Add to dropdown
  $('#job-family-dropdown').html(newFamilyHTML)

  /* COMPANY */
  // Create dropdown HTML
  var newOrganizationHTML = "<option value='-1'>Company</option>"

  for (var i = 0; i < relatedOrgs.data.length; i++) {
    // If multiple orgs under parent org, use opt group
    if (relatedOrgs.data[i].orgs.length > 1) {
      newOrganizationHTML += '<optgroup label="' + relatedOrgs.data[i].parentOrg + '">'

      for (var j = 0; j < relatedOrgs.data[i].orgs.length; j++) {
        newOrganizationHTML += "<option value='" + relatedOrgs.data[i].orgs[j] + "'>" + relatedOrgs.data[i].orgs[j] + '</option>'
      }

      newOrganizationHTML += '</optgroup>'
    } else {
      for (var j = 0; j < relatedOrgs.data[i].orgs.length; j++) {
        newOrganizationHTML += "<option value='" + relatedOrgs.data[i].orgs[j] + "'>" + relatedOrgs.data[i].orgs[j] + '</option>'
      }
    }
  }

  // Add to dropdown
  $('#company-dropdown').html(newOrganizationHTML)
}

function PopulateDropdownsFromS3() {
  // If we have keywords defined in the keyword search, add them to the sql statement

  if (savedS3Data === null) {
    let e = 'select * from s3object[*].locations [*] s ' //  let e = "select * from s3object[*].moc_info[*] s " + where;
    //let e = "select * from s3object[*].locations.data [*] s " + where;  //  let e = "select * from s3object[*].moc_info[*] s " + where;
    /*var t = document.getElementById("query-text").value;
        t.length > 5 && (e += " where " + t),*/
    //console.log(e);
    //_gaq.push(['_trackEvent','Related Civilian Credentials Search','click','Search Clicked - ' + e.toString(),,true]);
    let o = {
      Bucket: 'sb-data-solid', // make parameter
      Key: isLive ? liveLocationsFile : devLocationsFile, // make parameter        // moc_info.json
      Expression: e,
      ExpressionType: 'SQL',
      InputSerialization: {
        JSON: {
          Type: 'DOCUMENT',
        },
      },
      OutputSerialization: {
        JSON: {
          RecordDelimiter: ',',
        },
      },
      RequestProgress: {
        Enabled: !0,
      },
    }
    var n = ''
    s3.selectObjectContent(o, function (e, t) {
      if (e) console.log('error'), console.log(e, e.stack)
      else {
        var events = t.Payload
        //for (var s of o) {
        // refactored for ie 11
        for (var eve in events) {
          var s = events[eve]
          if (s.Records) {
            var r = s.Records.Payload
            i = new TextDecoder('utf-8').decode(r)
            n += i
          } else if (s.Stats);
          else if (s.Progress);
          else if (s.Cont);
          else if (s.End) {
            var a = '[' + n.slice(0, n.length - 1) + ']'
            c = JSON.parse(a)
            // console.log("c",c);
            //UpdateRelatedCivilianOccupationsTableData(c);
            //UpdateRelatedCivlianCredentialTableData(c);

            //console.log("returned data: " + c);
            //console.log("first object: " + c[0]);
            //console.log("first object id: " + c[0].ID);
            //ShowResultsContainer();
            //UpdateTableData(c);
            AddDataToDropdowns(c)
            savedS3Data = c
            HideLoadingScreen()
          }
        }
      }
    })
  } else {
    HideLoadingScreen()
  }
}

function AddDataToDropdowns(e) {
  var tabledata = e.map(function (r) {
    var info = {
      ACTIONS: "<button class='table-map-location' onclick='ShowPin(" + r.LAT + ',' + r.LONG + ',"' + r.CITY.replace(/,/g, ',') + '","' + r.STATE + '","' + r.ZIP + '","' + r.PROGRAM + '","' + r.EMPLOYERPOC + '","' + r.EMPLOYERPOCEMAIL + "\")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><img src='images/icon-map-location.png' alt='View Opportunity on Map'/></button>",
      //ACTIONS: "<a href='how-to-apply.htm' class='table-how-to-apply' style='color:#355e93;text-decoration:none !important;' data-service='" + r.SERVICE + "' data-installation='" + r.INSTALLATION + "' data-program='" + r.PROGRAM + "' title='How to Apply'><img src='images/icon-apply.png' alt='How to Apply'/></a><button class='table-map-location' onclick='ShowPin(" + r.LAT + ',' + r.LONG + ',"' + r.CITY.replace(/,/g, ',') + '","' + r.STATE + '","' + r.ZIP + '","' + r.PROGRAM + '","' + r.EMPLOYERPOC + '","' + r.EMPLOYERPOCEMAIL + "\")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><img src='images/icon-map-location.png' alt='View Opportunity on Map'/></button>",
      //APPLY: "<a href='how-to-apply.htm' style='color:#355e93;text-decoration:none !important;' data-service='" + r.SERVICE +  "' data-installation='" + r.INSTALLATION +  "' data-program='" + r.PROGRAM + "'><i class='fa-solid fa-clipboard-check' style='font-size: 18px;'></i></a>",
      //VIEWONMAP: "<button onclick='ShowPin(" + r.LAT + "," + r.LONG + ")' title='View Opportunity on Map' data-toggle='modal' data-target='#map-dialog' style='background: 0;border: 0;cursor: pointer;color:#355e93;text-decoration:none !important;'><i class='fa-solid fa-location-dot' style='font-size: 18px;'></i></button>",
      //APPLY: "<a href='how-to-apply.htm' class='btn-primary btn apply-btn' style='color:#ffffff;text-decoration:none !important;' data-service='" + r.SERVICE +  "' data-installation='" + r.INSTALLATION +  "' data-program='" + r.PROGRAM + "'>Apply</a>",
      //VIEWONMAP:  "<button onclick='ShowPin(" + r.LAT + "," + r.LONG + ")' class='btn-secondary btn apply-btn' style='color:#ffffff;text-decoration:none !important;font-weight:bold;' data-toggle='modal' data-target='#map-dialog'>View</button>",
      ORGANIZATION: r.ORGANIZATION,
      PROGRAM: r.PROGRAM,
      SERVICE: r.SERVICE,
      CITY: r.CITY,
      STATE: r.STATE,
      ZIP: r.ZIP,
      DURATIONOFTRAINING: r.DURATIONOFTRAINING,
      EMPLOYERPOC: r.EMPLOYERPOC,
      EMPLOYERPOCEMAIL: linkify(r.EMPLOYERPOCEMAIL),
      COST:adjustCost(r.COST),
      INSTALLATION: r.INSTALLATION,
      LOCATIONSOFPROSPECTIVEJOBSBYSTATE: r.LOCATIONSOFPROSPECTIVEJOBSBYSTATE,
      DELIVERY_METHOD: r.DELIVERY_METHOD,
      TARGETMOCs: r.TARGETMOCs,
      OTHERELIGIBILITYFACTORS: linkify(r.OTHERELIGIBILITYFACTORS),
      OTHER: linkify(r.OTHER),
      /*MOUs: r.MOUs,*/
      JOBSDESCRIPTION: linkify(r.JOBSDESCRIPTION),
      SUMMARYDESCRIPTION: linkify(r.SUMMARYDESCRIPTION),
      JOBFAMILIES: r.JOBFAMILIES,
      PARENTORGANIZATION: r.PARENTORGANIZATION,
    }
    return info
    //    }
  })

  PopulateDropdowns(tabledata)
}

function SelectSavedDropdownValues() {
  if (searchedProvider != '') {
    $('#provider-dropdown').val('-1')
    $('#provider-dropdown').val(searchedProvider)
  }
  if (searchedService != '') {
    $('#service-dropdown').val('-1')
    $('#service-dropdown').val(searchedService)
  }
  if (searchedDuration != '') {
    $('#duration-of-training-dropdown').val('-1')
    $('#duration-of-training-dropdown').val(searchedDuration)
  }
  if (searchedDelivery != '') {
    $('#delivery-method-dropdown').val('-1')
    $('#delivery-method-dropdown').val(searchedDelivery)
  }
  if (searchedLocProspective != '') {
    $('#location-of-prospective-jobs-dropdown').val('-1')
    $('#location-of-prospective-jobs-dropdown').val(searchedLocProspective)
  }
  if (searchedJobFamily != '') {
    $('#job-family-dropdown').val('-1')
    $('#job-family-dropdown').val(searchedJobFamily)
  }
  if (searchedCompany != '') {
    $('#company-dropdown').val('-1')
    $('#company-dropdown').val(searchedCompany)
  }
}

function SelectDefaultDropdownVals() {
  $('#provider-dropdown').val('')
  $('#service-dropdown').val('')
  $('#duration-of-training-dropdown').val('')
  $('#delivery-method-dropdown').val('')
  $('#location-of-prospective-jobs-dropdown').val('')
  $('#job-family-dropdown').val('')
  $('#company-dropdown').val('')
}

function SetupDataTables() {
  if ($('#location-table').length > 0) {
    // Look at each table header cell and set the index for each one
    $('#location-table thead th').each(function (index) {
      var columnName = $(this).text()
      SetColumnIndex(columnName, index)
    })

    table = $('#location-table').DataTable({
      retrieve: true,
      responsive: {
        details: {
          type: 'column',
          target: 0,
        },
      },
      rowGroup: {
        dataSrc: 1,
      },
      autoWidth: false,
      bAutoWidth: false,
      dom: "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
      order: [[1, 'asc']],
      columnDefs: [
        {
          className: 'control',
          targets: 0,
        },
        {
          targets: 1,
          searchable: true,
          visible: false,
        },
        {
          targets: 6,
          searchable: true,
          visible: false,
        },
      ],
      lengthMenu: [10, 25, 50],
      pageLength: 10
    })

    $.fn.DataTable.ext.pager.numbers_length = 5;

    /*$(".dataTables_paginate a").click(function(){
            SmoothScrollTo('#table-scroll-target');
        });*/

    table.on('page.dt', function () {
      SmoothScrollTo('#table-scroll-target')
    })
  }
}

function SmoothScrollTo(selector) {
  document.querySelector(selector).scrollIntoView({
    behavior: 'smooth',
  })
}

// Used to markup links in the table data
var urlRegex = /(\b(https?|ftp|file):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gi

function linkify(text) {
  return text.replace(urlRegex, function (url) {
    return '<a href="' + url + '" target="_blank">' + url + '</a>'
  })
}

function adjustCost(text)
{
  if(text === "") {
     text = "N/A"; //"No cost to participate";
  }
  
  return text;
}

function InitializeGoogleAnalyticsEvents() {
  return; // disable for now
  $('#loc-search-btn').click(function () {
    var keywords = $('#keywords').val()
    var location = $('#location').val()

    gtag('event', 'Search Triggered', {
      event_category: 'Locations Map',
      keywords: keywords,
      location: location,
      page_url: window.location.href
    });
  })
}

// this function turns a json object into arrays, without the keys
function json2arraynokeys(json) {
  var result = []
  var keys = Object.keys(json)

  keys.forEach(function (key) {
    //    result.push(Object.values(json[key]));  // pushes value only array to result array
    // vas 5/9/20 - refactored to work for IE 11
    var valarray = []
    for (var property in json[key]) {
      valarray.push(json[key][property])
    }
    result.push(valarray)
  })
  // console.log("result=",result)

  return result
}

function ShowResultsContainer() {
  $('#results-container').css('visibility', 'visible')
  $('#results-container').css('height', 'auto')
}

function HideResultsContainer() {
  $('#results-container').css('visibility', 'hidden')
}

function ShowMapContainer() {
  $('#map-container').css('visibility', 'visible')
}

function ShowLoadingScreen() {
  //alert("should be showing loading screen");
  $('.loading-screen-container').addClass('show')
  $('.loading-screen').addClass('show')
}

function HideLoadingScreen() {
  //alert("should be hiding loading screen");
  $('.loading-screen-container').removeClass('show')
  $('.loading-screen').removeClass('show')
}

function EnableTooltips() {
  $('[data-toggle="tooltip"]').tooltip({
    container: 'body',
  })
}

function abbrState(input, to) {
  var states = [
    ['Arizona', 'AZ'],
    ['Alabama', 'AL'],
    ['Alaska', 'AK'],
    ['Arkansas', 'AR'],
    ['California', 'CA'],
    ['Colorado', 'CO'],
    ['Connecticut', 'CT'],
    ['Delaware', 'DE'],
    ['Florida', 'FL'],
    ['Georgia', 'GA'],
    ['Hawaii', 'HI'],
    ['Idaho', 'ID'],
    ['Illinois', 'IL'],
    ['Indiana', 'IN'],
    ['Iowa', 'IA'],
    ['Kansas', 'KS'],
    ['Kentucky', 'KY'],
    ['Louisiana', 'LA'],
    ['Maine', 'ME'],
    ['Maryland', 'MD'],
    ['Massachusetts', 'MA'],
    ['Michigan', 'MI'],
    ['Minnesota', 'MN'],
    ['Mississippi', 'MS'],
    ['Missouri', 'MO'],
    ['Montana', 'MT'],
    ['Nebraska', 'NE'],
    ['Nevada', 'NV'],
    ['New Hampshire', 'NH'],
    ['New Jersey', 'NJ'],
    ['New Mexico', 'NM'],
    ['New York', 'NY'],
    ['North Carolina', 'NC'],
    ['North Dakota', 'ND'],
    ['Ohio', 'OH'],
    ['Oklahoma', 'OK'],
    ['Oregon', 'OR'],
    ['Pennsylvania', 'PA'],
    ['Rhode Island', 'RI'],
    ['South Carolina', 'SC'],
    ['South Dakota', 'SD'],
    ['Tennessee', 'TN'],
    ['Texas', 'TX'],
    ['Utah', 'UT'],
    ['Vermont', 'VT'],
    ['Virginia', 'VA'],
    ['Washington', 'WA'],
    ['West Virginia', 'WV'],
    ['Wisconsin', 'WI'],
    ['Wyoming', 'WY'],
  ]

  if (to == 'abbr') {
    input = input.replace(/\w\S*/g, function (txt) {
      return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase()
    })
    for (i = 0; i < states.length; i++) {
      if (states[i][0] == input) {
        return states[i][1]
      }
    }
  } else if (to == 'name') {
    input = input.toUpperCase()
    for (i = 0; i < states.length; i++) {
      if (states[i][1] == input) {
        return states[i][0]
      }
    }
  }
}

function capitalizeFirstLetter(string) {
  return string.charAt(0).toUpperCase() + string.slice(1)
}
