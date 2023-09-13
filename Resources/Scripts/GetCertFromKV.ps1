# If moduels need to be installed
# run
#       get-module AzureRM
#       Install-Module AzureRm.KeyVault  -Force -AllowClobber

Connect-AzureRmAccount -Tenant eadaaa2f-1d17-482b-8b97-839ec8e97361

$secretName = "CodeSigningCert092319"
$vaultName="KV-CDT-BPAS-D-001"
$kvSecret = Get-AzureKeyVaultSecret -VaultName $vaultName -Name $secretName
$kvSecretBytes = [System.Convert]::FromBase64String($kvSecret.SecretValueText)
$certCollection = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2Collection
$certCollection.Import($kvSecretBytes,$null,[System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)


#Get the file created
# $password = Read-Host 'What is the cert password?' -AsSecureString #"pwd"
$protectedCertificateBytes = $certCollection.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12) #, $password)
$pfxPath = [Environment]::GetFolderPath("Desktop") + "\MyCert.pfx"
[System.IO.File]::WriteAllBytes($pfxPath, $protectedCertificateBytes)

$cert = Get-AzureKeyVaultCertificate -VaultName $vaultName -Name $secretName
$filePath = [Environment]::GetFolderPath("Desktop") + "\MyCert.cer"
$certBytes = $cert.Certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
[System.IO.File]::WriteAllBytes($filePath, $certBytes)