# Ai plugin, making AI accessible to everyone - even your grandma!

all nice stuffs here

## Migrate
migrations with 
```
dotnet ef migrations add "XYZ"  --output-dir .\Migrations\ --namespace AiPlugin.Migrations --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -- --env Development
```

apply them with 

```
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\Api\Api.csproj -- --env Development 
```

## Safe HTTPS Subdomain (wildcard SSL certificate)
*The problem*   
Azure subdomains certificates cost 200$ so no UserName.Genesi.ai for us.   
*The solution*  
Generate a and install manually a certificate with [certbot](https://certbot.eff.org) and [OpenSSL](https://www.openssl.org/) every 2 months and a half.
### Install Certbot
download and install it from https://certbot.eff.org/.

### 1 Generate Wildcard Certificate
#### 1.1 Run Certbot

``` bash
certbot certonly --manual --preferred-challenges=dns --email michele@bortot.dev --server https://acme-v02.api.letsencrypt.org/directory --agree-tos -d *.genesi.ai 
```
The --manual flag tells Certbot to provide instructions for manual DNS configuration. The --preferred-challenges=dns flag specifies that domain ownership should be verified via DNS. The -d flag specifies the domain for which to generate the certificate.

Certbot will provide you with a DNS TXT record to add to your domain's DNS settings. 
``` bash
Please deploy a DNS TXT record under the name:

_acme-challenge.genesi.ai.   // NAME TO COPY

with the following value:

IRkf6Jg2QslkHrjnKx_jeYi7AZDJucW7g5Vs5b-CHfs  // VALUE TO COPY
```
This TXT record is used by Let's Encrypt to verify domain ownership. 
#### 1.2 Verify Domain Ownership
Go on [godaddy's genesi.ai dns page](https://dcc.godaddy.com/manage/genesi.ai/dns) and add a TXT record with the name and value provided by Certbot.
press ENTER in Certbot to continue the process.
it should say something like:

``` bash
Successfully received certificate.
Certificate is saved at: C:\Certbot\live\genesi.ai\fullchain.pem
Key is saved at:         C:\Certbot\live\genesi.ai\privkey.pem
This certificate expires on 2023-08-16.
These files will be updated when the certificate renews.
```

### 2 Wrap Certificates in PFX File
Now you need openSSL, do not download it from the website, it's already installed on your machine (if you have git installed). usually at C:\Program Files\Git\usr\bin\openssl.exe so you should be able to runn it from the command line.
Go to the directory Certbot has saved the certificates, `C:\Certbot\live\genesi.ai\` usually.
and run 
``` bash
 .'..\..\..\Program Files\Git\usr\bin\openssl.exe'
```
and than
``` bash
openssl pkcs12 -export -out certificate.pfx -inkey privkey.pem -in fullchain.pem
```
It should ask you for a password, provide it, remember it, you'll need it later.

### Upload Certificate to Azure:
[Log in to the Azure portal](https://portal.azure.com/), navigate to the App Service (should be aiplugin-api) > "cetificates" > "Add a certificate" > "upload certificate"> 
Select the .pfx file, enter the password you used when exporting the certificate> "Validate".
select "Custom Domains". Than in the list open the contest menu (...) of *.genesi.ai and select "Update binding" and select the new certificate.
Remember, Let's Encrypt certificates are valid for 90 days. You'll need to repeat this process every 90 days to renew your certificate.
