<?php

$url = "https://raw.githubusercontent.com/tmurakam/felica2money/master/defs/RecentVersion.txt";
//$url = "http://www.yahoo.com";

$conn = curl_init($url);

//curl_setopt($conn, CURLOPT_SSL_VERIFYPEER, true);
//curl_setopt($conn, CURLOPT_SSL_VERIFYHOST, true);
curl_setopt($conn, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($conn, CURLOPT_SSL_VERIFYHOST, false);
curl_setopt($conn, CURLOPT_SSLVERSION, 3);

curl_setopt($conn, CURLOPT_CONNECTTIMEOUT, 2);
curl_setopt($conn, CURLOPT_FOLLOWLOCATION, 1);
curl_setopt($conn, CURLOPT_RETURNTRANSFER, 1);
curl_setopt($conn, CURLOPT_HEADER, false);
//curl_setopt($conn, CURLOPT_USERAGENT, "curl");
curl_setopt($conn, CURLOPT_VERBOSE, false);

$response = curl_exec($conn);

curl_close($conn);

print $response;
?>
