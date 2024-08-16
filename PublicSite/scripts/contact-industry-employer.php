<?php
// Include PHPMailer
use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

require_once('Exception.php');
require_once('PHPMailer.php');
require_once('SMTP.php');

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Check if encodedData is set in POST request
    if(isset($_POST['encodedData'])) {
        // Decode the Base64 encoded data
        $base64EncodedData = $_POST['encodedData'];
        $serializedData = base64_decode($base64EncodedData);

        // Unserialize the data into $_POST array
        parse_str($serializedData, $_POST);
    }
}

// Define Mail Object
$mail = new PHPMailer(true);    // Passing `true` enables exceptions

// EDIT THE 2 LINES BELOW AS REQUIRED
$email_subject_base = "SkillBridge Industry Employer Contact Form Submission - [MOU]";
$app_email_subject = "[APP]";
$army_email_subject = "[ARMY]";
$navy_email_subject = "[NAVY]";
$af_email_subject = "[AF]";
$mc_email_subject = "[MC]";
$cg_subj_add = "[CG]";
$email_subject_final = "";
$ticket_email = "support@skillbridge.zohodesk.com";//"dodskillbridgeassistancecenter@livehelpnow.net";
$osd_email = "OSD-SkillBridge@skillbridge.zohodesk.com";
$af_email = "AF-SkillBridge@skillbridge.zohodesk.com";
$army_email = "Army-SkillBridge@skillbridge.zohodesk.com";
$cg_email = "CG-SkillBridge@skillbridge.zohodesk.com";
$help_email = "Help-SkillBridge@skillbridge.zohodesk.com";
$mc_email = "MC-SkillBridge@skillbridge.zohodesk.com";
$navy_email = "Navy-SkillBridge@skillbridge.zohodesk.com";

// ReCaptcha Response
$recaptcha_response = $_POST['g-recaptcha-response'];

// Interested Service Member Form Data
$applying = isset($_POST['form2-apply']) ? $_POST['form2-apply'] : '0';
$company_name = $_POST['form2-company'];
$first_name = $_POST['form2-first-name']; 
$last_name = $_POST['form2-last-name']; 
$industry = $_POST['form2-industry'];
$experience = $_POST['form2-experience'];
$description = $_POST['form2-description'];
$branch = $_POST['form2-branch'];
$willing = isset($_POST['form2-willing']) ? $_POST['form2-willing'] : '0';
$specific_training = isset($_POST['form2-specific-training']) ? $_POST['form2-specific-training'] : '0';		
$personnel_description = $_POST['form2-personnel-description'];
$comment = $_POST['form2-comment'];
$telephone = $_POST['form2-phone'];
$email_from = $_POST['form2-email'];

// Generate a string to display for "I am willing to retrain military personnel"
$is_willing;
if(!empty($_POST['form2-willing'])){
    $is_willing = "yes";
}
else
{
   $is_willing = "no"; 
}

// Generate a string to display for "I am looking for military personnel with specific training and experience (please describe): (e.g., plumbing, IT)"
$has_specific_training;
if(!empty($_POST['form2-specific-training'])){
    $has_specific_training = "yes";
}
else
{
   $has_specific_training = "no"; 
}

// Create the message body, populate it with form data
$email_message_final = "<html><body>#original_sender {" . $email_from . "}<p style='font-weight:bold;font-size:18px;margin-bottom:20px;'>".clean_string($company_name)." - Interested Industry Employer or Agency Form Details Below.</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Company or Organization Name: ".clean_string($company_name)."</p>"; 

$email_message_final .= "<p><span style='font-weight:bold;'>First Name:</span> ".clean_string($first_name)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Last Name:</span> ".clean_string($last_name)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Type of Industry or Agency:</span> ".clean_string($industry)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Seeking Level of Experience:</span> ".clean_string($experience)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Brief Description of Company or Agency's Mission and Services:</span> ".clean_string($description)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Branch(es) of Service Interested In:</span> ".implode(", ",$branch)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Willing to retrain military personnel:</span> $is_willing</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Looking for military personnel with specific training and experience: </span>$has_specific_training</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Personnel Description:</span> ".clean_string($personnel_description)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Comment / Question:</span> ".clean_string($comment)."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Phone Number:</span> ".$telephone."</p>";

$email_message_final .= "<p><span style='font-weight:bold;'>Email Address:</span> ".clean_string($email_from)."</p></body></html>";


// Validate recaptcha and then create a submission email/ticket
if (validate_recaptcha($recaptcha_response) == true) 
{
    //Server settings
    $mail->SMTPDebug = 2;                                 // Enable verbose debug output
    $mail->isSMTP();                                      // Set mailer to use SMTP
    $mail->Host = '';                      // Specify main and backup SMTP servers
    $mail->SMTPAuth = true;                               // Enable SMTP authentication
    $mail->Username = ''; // SMTP username
    $mail->Password = '';                      // SMTP password
    $mail->Port = 25;                                     // TCP port to connect to
    $mail->SMTPKeepAlive = true;

    // Recipients (Email From is used by LHN to notify the sender of ticket updates)
    $mail->setFrom($email_from, $first_name." ".$last_name);

    $ticket_email = $osd_email;

    $mail->addAddress($ticket_email);     // LiveHelpNow Email-to-Ticket email address

    // Content
    $mail->isHTML(true);   // Set email format to HTML
    
    /*
    // Set the files
    $file_tmp  = $_FILES['form2-mou']['tmp_name'];
    $file_name = $_FILES['form2-mou']['name'];
    
    // Check File MIME Type
    if($_FILES['form2-mou']['type'] == 'application/pdf')
    {
        // Check file size
        if ($_FILES["form2-mou"]["size"] <= 2097152) {
            $mail->addAttachment($file_tmp, $file_name);
        }
    }*/
    
    // Start creating the email subject line, including the [MOU] flag (required for all MOU submissions regardless of department/service)
    $email_subject_final = $email_subject_base;

    // Industry form can go to multiple service departments, add string flag for each one that was selected on the form to the subject line ([NAVY], [ARMY], [AF], [MC])
    if(in_array("Army", $branch) == 1)
    {
        $email_subject_final .= $army_email_subject;
    }
    
    if(in_array("Navy", $branch) == 1)
    {                     
        $email_subject_final .= $navy_email_subject;
    }
    
    if(in_array("Air Force", $branch) == 1)
    {                        
        $email_subject_final .= $af_email_subject;
    }        
    
    if(in_array("Marine Corps", $branch) == 1)
    {                        
        $email_subject_final .= $mc_email_subject;
    }

    if(in_array("Coast Guard", $branch) == 1)
    {                        
        $email_subject_final .= $cg_subj_add;
    }
    
    // If they intend to apply, append the [MOU] subject line tag
    if(!empty($_POST['form2-apply'])){
        $email_subject_final .= $app_email_subject;
    }    
    
    // Create the email with the final generated subject line and message
    try 
    {
        $mail->Subject = $email_subject_final;
        $mail->Body    = $email_message_final;
        $mail->send();
    } 
    catch (Exception $e) 
    {
        //echo 'Message could not be sent. Mailer Error: ', $mail->ErrorInfo;
    }
    
    // Close the connection
    $mail->SmtpClose();
}

// Clean a string that will end up getting displayed
function clean_string($string) 
{
  $bad = array("content-type","bcc:","to:","cc:","href");
  return str_replace($bad,"",$string);
}
  
// Validate the recaptcha challenge response
function validate_recaptcha($response)
{
    // Verifying the user's response (https://developers.google.com/recaptcha/docs/verify)
    $verifyURL = 'https://www.google.com/recaptcha/api/siteverify';

    // Collect and build POST data
    $post_data = http_build_query(
        array(
            'secret' => '',
            'response' => $response,
            'remoteip' => (isset($_SERVER["HTTP_CF_CONNECTING_IP"]) ? $_SERVER["HTTP_CF_CONNECTING_IP"] : $_SERVER['REMOTE_ADDR'])
        )
    );

    // Send data on the best possible way
    if(function_exists('curl_init') && function_exists('curl_setopt') && function_exists('curl_exec')) {
        // Use cURL to get data 10x faster than using file_get_contents or other methods
        $ch =  curl_init($verifyURL);
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_POSTFIELDS, $post_data);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);
            curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
            curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 5);
            curl_setopt($ch, CURLOPT_TIMEOUT, 5);
            curl_setopt($ch, CURLOPT_HTTPHEADER, array('Accept: application/json', 'Content-type: application/x-www-form-urlencoded'));
            $response = curl_exec($ch);
        curl_close($ch);
    } else {
        // If server not have active cURL module, use file_get_contents
        $opts = array('http' =>
            array(
                'method'  => 'POST',
                'header'  => 'Content-type: application/x-www-form-urlencoded',
                'content' => $post_data
            )
        );
        $context  = stream_context_create($opts);
        $response = file_get_contents($verifyURL, false, $context);
    }

    // Verify all reponses and avoid PHP errors
    if($response) {
        $result = json_decode($response);
        if ($result->success===true) {
            return true;
        } else {
            //echo "reCAPTCHA check failed. Please fill check the reCAPTCHA box and resubmit the form.";
            return false;
        }
    }

    // Dead end
    return false; 
}

?>