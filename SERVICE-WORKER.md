# Service worker not available

Simply put Service worker (SW) is a script (piece of code) that can run in the background even when the web app isn't open. 
That is a requirement to be able to recieve and show push notifications - the web app might not be open when one arrives.

To be able to do that the SW needs to register with the browser (e.g. Firefox) first, which the browser can deny.
The most common situation when this happens is when the web app is running on an unsecured HTTP connection. 
Because the SW is a fairly powerful tool which can do many (including malicious) things, HTTPS is required. 

Settings up HTTPS on your home network (where your apps might be running e.g. on Raspberry Pi) is not an easy thing to do, 
but if you want to try, see these:
[[1]](https://serverfault.com/questions/1060268/ssl-for-devices-in-local-network)
[[2]](https://serverfault.com/questions/906015/how-to-setup-ssl-certs-for-a-lan-web-app-server)
[[3]](https://serverfault.com/questions/964119/enable-https-on-a-private-network)
[[4]](https://serverfault.com/questions/573528/ssl-tls-cert-get-alternative-name-to-work-with-lan-ip)
[[5]](https://serverfault.com/questions/833178/ssl-with-no-warning-for-local-ips)
[[6]](https://serverfault.com/questions/447753/ssl-certificate-for-local-web-server).

The easy way out is to disable the HTTPS requirement in your browser.

## Firefox
Navigate to `about:config` and search for `devtools.serviceWorkers.testing.enabled`. 
Make sure it's set to `true`. 

## Chrome
Navigate to `chrome://flags` and search for `Insecure origins treated as secure`. 
Enter the url you use to access AutoSats (e.g. `http://umbrel.local:3311` or `http://192.168.0.100:3311`) and make sure it's set to `Allowed`.

## Edge
Navigate to `edge://flags` and search for `Insecure origins treated as secure`. 
Enter the url you use to access AutoSats (e.g. `http://umbrel.local:3311` or `http://192.168.0.100:3311`) and make sure it's set to `Allowed`.