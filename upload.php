<?php
$db_host = 'localhost'; // don't forget to change 
$db_user = 'root'; 
$db_pwd = 'P@ssw0rd';

$database = 'GlurDB';
$table = 'product';
// use the same name as SQL table

//$password = '123';
// simple upload restriction,
// to disallow uploading to everyone


if (!mysql_connect($db_host, $db_user, $db_pwd))
    die("Can't connect to database");

if (!mysql_select_db($database))
    die("Can't select database");

// This function makes usage of
// $_GET, $_POST, etc... variables
// completly safe in SQL queries
function sql_safe($s)
{
    if (get_magic_quotes_gpc())
        $s = stripslashes($s);

    return mysql_real_escape_string($s);
}

// If user pressed submit in one of the forms
    
        if(count($_FILES['photo']['name']) > 0)
        {
            //Loop through each file
            for($i=0; $i<count($_FILES['photo']['name']); $i++) {

        if (isset($_FILES['photo']))
        {
            @list(, , $imtype, ) = getimagesize($_FILES['photo']['tmp_name'][$i]);
            
            // Get image type.
            // We use @ to omit errors

            if ($imtype == 3) // cheking image type
                $ext="png";   // to use it later in HTTP headers
            elseif ($imtype == 2)
                $ext="jpeg";
            elseif ($imtype == 1)
                $ext="gif";
            else
                $msg = 'Error: unknown file format';

            if (!isset($msg)) // If there was no error
            {
                $data = file_get_contents($_FILES['photo']['tmp_name'][$i]);
                $data = mysql_real_escape_string($data);
                $filename = $_FILES['photo']['name'][$i];  

                $Desp = $_POST["Desp"];
                $Price= $_POST["Price"];
                $Per=   $_POST["Per"];
                $Header=$_POST["Header"];
                // Preparing data to be used in MySQL query               
                //mysql_query("INSERT INTO {$table}
                //                SET ext='$ext', title='Test',
                //                   data='$data'");

                mysql_query("INSERT INTO {$table}
                                SET Image   ='$data', 
                                    Name    ='$filename',
                                    Type    ='$ext',
                                    Desp    ='$Desp',
                                    Price   ='$Price',
                                    Per     ='$Per',
                                    Header  ='$Header'");

                $msg = 'Success: image uploaded';
            }
        }
        if (isset($_POST['del'])) // If used selected some photo to delete
        {                         // in 'uploaded images form';
            $id = intval($_POST['del']);
            mysql_query("DELETE FROM {$table} WHERE id=$id");
            $msg = 'Photo deleted';
        }
    }
}

?>
<html><head>
<title>MySQL Blob Image Gallery Example</title>
</head>
<body>
<?php
if (isset($msg)) // this is special section for
                 // outputing message
{


            ?>
            <p style="font-weight: bold;"><?=$msg?>
            <br>
            <a href="<?=$PHP_SELF?>">reload page</a>

            </p>
            <?php
}
?>
<h1>Blob image gallery</h1>
<h2>Uploaded images:</h2>
<h2>Upload new image:</h2>
<form action="<?=$PHP_SELF?>" method="POST" enctype="multipart/form-data">

<label for="photo">Photo:</label><br>
<input type="file" name="photo[]" id="photo" multiple="multiple"><br>
<input name="Desp" type="text" class="widebox" id="Desp"><br>
<input name="Price" type="text" class="widebox" id="Price"><br>
<input name="Per" type="text" class="widebox" id="Per"><br>
<input name="Header" type="text" class="widebox" id="Header"><br>

<input type="submit" value="upload">
</form>
</body>
</html>