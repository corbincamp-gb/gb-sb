<?php
// Include PHPMailer
use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

require_once('Exception.php');
require_once('PHPMailer.php');
require_once('SMTP.php');

// Define Mail Object
$mail = new PHPMailer(true);                              // Passing `true` enables exceptions

// EDIT THE 2 LINES BELOW AS REQUIRED
$army_subj_add= "[ARMY]";
$navy_subj_add = "[NAVY]";
$af_subj_add = "[AF]";
$mc_subj_add = "[MC]";
$cg_subj_add = "[CG]";
$provider_subj_add = "[Provider]";
$change_request_subj_add = "[CMSChangeRequest]";
$report_issue_add = "[Issue]";
$email_subject = "SkillBridge General Contact Form Submission - ";
$update_subject = "SkillBridge Update Provider/Opportunity Information - DO NOT REPLY";
$ticket_email = "support@skillbridge.zohodesk.com";//"dodskillbridgeassistancecenter@livehelpnow.net";
$noreply_email = "skillbridge@skillbridge.org";
$osd_email = "OSD-SkillBridge@skillbridge.zohodesk.com";
$af_email = "AF-SkillBridge@skillbridge.zohodesk.com";
$army_email = "Army-SkillBridge@skillbridge.zohodesk.com";
$cg_email = "CG-SkillBridge@skillbridge.zohodesk.com";
$help_email = "Help-SkillBridge@skillbridge.zohodesk.com";
$mc_email = "MC-SkillBridge@skillbridge.zohodesk.com";
$navy_email = "Navy-SkillBridge@skillbridge.zohodesk.com";

// ReCaptcha Response
$recaptcha_response = $_POST['g-recaptcha-response'];

// General Contact Form Data
$first_name = $_POST['form4-first-name']; // required
$last_name = $_POST['form4-last-name']; // required
$email_from = $_POST['form4-email']; // required
$telephone = $_POST['form4-phone']; // required
$comments = $_POST['form4-comment']; // required
$contact_type = $_POST['form4-contact-type']; // required
$provider_name = $_POST['form4-provider-name']; // required, but defaults to "No Provider Given" via JS if not needed
$contact_purpose = $_POST['form4-contact-purpose']; // required

// Create the message body, populate it with form data
$email_message = "<html><body><p style='font-weight:bold;font-size:18px;margin-bottom:20px;'>General Inquiry Form Details Below.</p>";

$email_message .= "<p><span style='font-weight:bold;'>Contact Type:</span> ".clean_string($contact_type)."</p>";

if($contact_type == "Provider")
{
    $email_message .= "<p><span style='font-weight:bold;'>Provider Name:</span> ".clean_string($provider_name)."</p>";
}

$email_message .= "<p><span style='font-weight:bold;'>Contact Purpose:</span> ".clean_string($contact_purpose)."</p>";

$email_message .= "<p><span style='font-weight:bold;'>First Name:</span> ".clean_string($first_name)."</p>";
$email_message .= "<p><span style='font-weight:bold;'>Last Name:</span> ".clean_string($last_name)."</p>";
$email_message .= "<p><span style='font-weight:bold;'>Email Address:</span> ".clean_string($email_from)."</p>";
$email_message .= "<p><span style='font-weight:bold;'>Phone Number:</span> ".clean_string($telephone)."</p>";
$email_message .= "<p><span style='font-weight:bold;'>Comments:</span> ".clean_string($comments)."</p></body></html>";


// Create the update web site provider/opportunity information email body
$update_information .= "<html><body><p>Thank you for your message</p>";
$update_information .= "<p>If you would like to update any information regarding your organization, program(s), and or opportunity(s), please complete the following questionnaire: <a href='https://skillbridgeprogramdetailsrefresh.questionpro.com/' title='DoD SkillBridge Industry Partner Program Details Form'>https://skillbridgeprogramdetailsrefresh.questionpro.com/</a> and the change will be updated as soon as possible. A member of the SkillBridge will contact you directly if there are questions regarding your requested changes.</p>";
$update_information .= "<p>Thank you for your continued support of our Service Members,<br/>";
$update_information .= "- The SkillBridge Team</p>";
$update_information .= "<br\><br\><p style='font-weight:bold;'>DO NOT REPLY TO THIS EMAIL<br/>This email was sent from an account that we use for sending email messages only, please don't reply to this email.</p></body></html>";

$update_alt .= "Thank you for your message";
$update_alt .= "If you would like to update any information regarding your organization, program(s), and or opportunity(s), please complete the following questionnaire: https://skillbridgeprogramdetailsrefresh.questionpro.com/ and the change will be updated as soon as possible. A member of the SkillBridge will contact you directly if there are questions regarding your requested changes.";
$update_alt .= "Thank you for your continued support of our Service Members,";
$update_alt .= "- The SkillBridge Team";
$update_alt .= "DO NOT REPLY TO THIS EMAIL<br/>This email was sent from an account that we use for sending email messages only, please don't reply to this email.";

// Validate recaptcha and then create a submission email/ticket
if (validate_recaptcha($recaptcha_response) == true)
{
    // Create the email
    try 
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

        $new_ticket_email = $ticket_email;

        if($contact_type == 'Servicemember - Army')
        {
            $email_subject .= $army_subj_add;
            $new_ticket_email = $army_email;
        }
        else if($contact_type == 'Servicemember - Navy')
        {
            $email_subject .= $navy_subj_add;
            $new_ticket_email = $navy_email;
        }
        else if($contact_type == 'Servicemember - Air Force')
        {
            $email_subject .= $af_subj_add;
            $new_ticket_email = $af_email;
        }
        else if($contact_type == 'Servicemember - Marine Corps')
        {
            $email_subject .= $mc_subj_add;
            $new_ticket_email = $mc_email;
        }
        else if($contact_type == 'Servicemember - Coast Guard')
        {
            $email_subject .= $cg_subj_add;
            $new_ticket_email = $cg_email;
        }
        else if($contact_type == 'Provider')
        {
            $email_subject .= $provider_subj_add;
            $new_ticket_email = $osd_email;

            if($contact_purpose == 'Update Provider/Opportunity Information')
            {
                $email_subject .= $change_request_subj_add;
            }
            else if($contact_purpose == 'Report an Issue')
            {
                $email_subject .= $report_issue_add;
            }
        }

        //Content
        $mail->isHTML(true);                                  // Set email format to HTML

        /*if($contact_purpose == "Update Provider/Opportunity Information")
        {
            $mail->addAddress($email_from);       // Send direct email
            $mail->Body = $update_information;     // Send direct email body
            $mail->Subject = $update_subject; 
            $mail->setFrom($noreply_email, "SkillBridge");  //Recipients
            $mail->AltBody = $update_alt;
        }
        else
        {*/
            $mail->addAddress($new_ticket_email);     // Add ticket creation email recipient
            $mail->Body = $email_message;     // Add ticket creation body
            $mail->Subject = $email_subject;
            $mail->setFrom($email_from, $first_name." ".$last_name);  //Recipients
        //}
        
        //$mail->addAddress('ellen@example.com');               // Name is optional
        //$mail->addReplyTo('info@example.com', 'Information');
        //$mail->addCC('cc@example.com');
        //$mail->addBCC('bcc@example.com');

        //Attachments
        //$mail->addAttachment('/tmp/image.jpg', 'new.jpg');    // Optional name
        

        $mail->send();
        //echo 'Message has been sent';
    } 
    catch (Exception $e) 
    {
        //echo 'Message could not be sent. Mailer Error: ', $mail->ErrorInfo;
    }
}

function clean_string($string) 
{
  $bad = array("content-type","bcc:","to:","cc:","href");
  return str_replace($bad,"",$string);
}
    
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