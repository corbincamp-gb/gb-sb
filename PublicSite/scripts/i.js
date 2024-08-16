// Ensure credentials (IAM User) are restricted in use to a specific domain or URL,
// Only allow GET requests, and
// Surface public data that's accessible elsewhere.


var config = new AWS.Config({
    // apiVersion: "2006-03-01", // optional
    // endpoint: "https://s3.amazonaws.com",  // doesn't seem to be needed, must be known by the SDK
    accessKeyId: "",
    secretAccessKey: "",
    region: ""
  })
  
  // constructing a S3 object
  var s3 = new AWS.S3(config)