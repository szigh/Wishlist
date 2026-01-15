# PowerShell script to populate the Wishlist database with test data
# Run this script after starting the API with docker-compose

[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$UserCount = 10,
    [int]$GiftsPerUser = 5
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Wishlist Database Population Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Users to create: $UserCount" -ForegroundColor Yellow
Write-Host "Gifts per user: $GiftsPerUser" -ForegroundColor Yellow
Write-Host ""

# Sample data for generating gifts
$giftCategories = @("Electronics", "Books", "Clothing", "Toys", "Home", "Sports", "Music", "Gaming", "Art", "Kitchen")
$giftTitles = @(
    "Wireless Headphones", "Smart Watch", "Laptop Stand", "Mechanical Keyboard", "USB-C Hub",
    "The Hobbit Book", "Programming Guide", "Mystery Novel", "Cookbook", "Art Book",
    "Winter Jacket", "Running Shoes", "Wool Sweater", "Baseball Cap", "Hiking Boots",
    "Board Game", "Puzzle Set", "Building Blocks", "RC Car", "Action Figure",
    "Coffee Maker", "Desk Lamp", "Wall Art", "Indoor Plant", "Photo Frame",
    "Yoga Mat", "Tennis Racket", "Bicycle Helmet", "Camping Tent", "Water Bottle",
    "Guitar Strings", "Piano Book", "Vinyl Record", "Concert Tickets", "Music Stand",
    "Gaming Mouse", "Controller", "Gaming Chair", "VR Headset", "Game Collection",
    "Paint Set", "Sketch Book", "Clay Tools", "Canvas Pack", "Easel",
    "Blender", "Pan Set", "Knife Set", "Spice Rack", "Cutting Board"
)

$descriptions = @(
    "Would love to have this!", "Really need this for my hobby", "Been wanting this for a while",
    "Perfect for my collection", "This would be amazing", "Great quality product",
    "Highly recommended by friends", "Would make me very happy", "Essential for daily use",
    "Perfect gift idea", "Really useful for my work", "Would complete my setup",
    "Been on my wishlist forever", "Exactly what I need", "Would be perfect for weekends"
)

$stores = @("Amazon", "BestBuy", "Target", "Walmart", "Etsy", "eBay")

# Function to make API calls
function Invoke-ApiCall {
    param(
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Body = $null,
        [string]$Token = $null
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    $params = @{
        Uri = "$BaseUrl$Endpoint"
        Method = $Method
        Headers = $headers
    }
    
    if ($Body) {
        $params["Body"] = ($Body | ConvertTo-Json)
    }
    
    try {
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Error calling $Endpoint : $_" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# Arrays to store created users and gifts
$users = @()
$allGifts = @()

Write-Host "Creating users and their wishlists..." -ForegroundColor Green
Write-Host ""

# Create a unique suffix to avoid username conflicts
$timestamp = Get-Date -Format "MMddHHmmss"

# Create users and their gifts
for ($i = 1; $i -le $UserCount; $i++) {
    $username = "user${i}_$timestamp"
    $password = "Password123!"
    
    Write-Host "[$i/$UserCount] Creating user: $username" -ForegroundColor Cyan
    
    # Register user
    $registerBody = @{
        Name = $username
        Password = $password
    }
    
    $registerResponse = Invoke-ApiCall -Method "POST" -Endpoint "/api/auth/register" -Body $registerBody
    
    if ($registerResponse) {
        $user = @{
            UserId = $registerResponse.userId
            Username = $registerResponse.name
            Token = $registerResponse.token
        }
        $users += $user
        
        $userId = $user.UserId
        Write-Host "  User created (ID: $userId)" -ForegroundColor Green
        
        # Create gifts for this user
        $userGifts = @()
        for ($j = 1; $j -le $GiftsPerUser; $j++) {
            $giftIndex = Get-Random -Minimum 0 -Maximum $giftTitles.Count
            $categoryIndex = Get-Random -Minimum 0 -Maximum $giftCategories.Count
            $descIndex = Get-Random -Minimum 0 -Maximum $descriptions.Count
            $storeIndex = Get-Random -Minimum 0 -Maximum $stores.Count
            
            $storeName = $stores[$storeIndex].ToLower()
            $giftBody = @{
                Title = "$($giftTitles[$giftIndex]) #$j"
                Description = $descriptions[$descIndex]
                Link = "https://www.$storeName.com/product/$giftIndex"
                Category = $giftCategories[$categoryIndex]
            }
            
            $gift = Invoke-ApiCall -Method "POST" -Endpoint "/api/gift" -Body $giftBody -Token $user.Token
            
            if ($gift) {
                $userGifts += $gift
                $giftTitle = $gift.title
                $giftId = $gift.id
                Write-Host "    Gift created: $giftTitle (ID: $giftId)" -ForegroundColor Gray
            }
        }
        
        $allGifts += $userGifts
        Write-Host ""
    }
    else {
        Write-Host "  Failed to create user" -ForegroundColor Red
        Write-Host ""
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creating volunteers (claiming gifts)..." -ForegroundColor Green
Write-Host ""

# Have users volunteer for each other's gifts
$volunteeredCount = 0
foreach ($user in $users) {
    $volunteersToCreate = Get-Random -Minimum 2 -Maximum 4
    
    # Get gifts from other users that are not taken
    $otherUsersGifts = @()
    foreach ($gift in $allGifts) {
        if (($gift.userId -ne $user.UserId) -and ($gift.isTaken -eq $false)) {
            $otherUsersGifts += $gift
        }
    }
    
    if ($otherUsersGifts.Count -gt 0) {
        $giftsToVolunteer = $otherUsersGifts | Get-Random -Count ([Math]::Min($volunteersToCreate, $otherUsersGifts.Count))
        
        foreach ($gift in $giftsToVolunteer) {
            $volunteerBody = @{
                GiftId = $gift.id
            }
            
            $volunteer = Invoke-ApiCall -Method "POST" -Endpoint "/api/volunteers" -Body $volunteerBody -Token $user.Token
            
            if ($volunteer) {
                $volunteeredCount++
                $giftTitle = $gift.title
                $userName = $user.Username
                Write-Host "  $userName volunteered for: $giftTitle" -ForegroundColor Green
            }
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Population Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Yellow
$userCountTotal = $users.Count
$giftCountTotal = $allGifts.Count
Write-Host "  Users created: $userCountTotal" -ForegroundColor White
Write-Host "  Gifts created: $giftCountTotal" -ForegroundColor White
Write-Host "  Gifts claimed: $volunteeredCount" -ForegroundColor White
Write-Host ""
Write-Host "You can now log in with any of these users:" -ForegroundColor Yellow
Write-Host "  Username: user1_$timestamp to user${UserCount}_$timestamp" -ForegroundColor White
Write-Host "  Password: Password123!" -ForegroundColor White
Write-Host ""
