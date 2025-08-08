# Game Picker Challenge 🕹️

RESTful API for suggesting games based on filters like genre, platform, and RAM requirements. The API integrates with external gaming APIs to provide personalized game recommendations.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose
- Git
- MongoDB (provided via Docker)
- Redis (provided via Docker)

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd GamePickerChallenge
   ```

2. **Start MongoDB and Redis with Docker**
   ```bash
   docker-compose up -d
   ```

3. **Run the API Locally**
   ```bash
   cd GamePicker
   dotnet run
   ```

4. **Access the API**
   - API Base URL: `http://localhost:7018`
   - API URL Through Docker: `http://localhost:5000`
   - Swagger Documentation: `http://localhost:7018/swagger`

## 📋 API Endpoints

### 1. Get Game Recommendation
**POST** `/recommendation`

Get a personalized game recommendation based on specified filters.

#### Request Body
```json
{
  "genres": ["Shooter", "RPG", "Strategy"],
  "platform": "pc",
  "ramGb": 8
}
```

#### Parameters
- `genres` (required): Array of game genres. At least one genre is required.
- `platform` (optional): Platform to filter games by.
  - Options: `"pc"`, `"browser"`, `"all"`
  - Default: `"all"`
- `ramGb` (optional): Maximum RAM in GB to filter games by system requirements.
  - Type: `integer`
  - Example: `8`

#### Response
```json
{
  "data": {
    "title": "Valorant",
    "linkFromApiSite": "https://www.freetogame.com/games/valorant",
    "message": "Do you like shooter? You definitely should play Valorant!"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

#### Status Codes
- `200`: Success - Game recommendation returned
- `400`: Bad Request - Invalid parameters
- `404`: Not Found - No games found matching criteria
- `500`: Internal Server Error

### 2. Get Game Recommendations History
**GET** `/recommendations/history`

Get paginated history of game recommendations with sorting options.

#### Query Parameters
- `pageSize` (optional): Number of items per page. Must be between 1 and 100.
  - Type: `integer`
  - Default: `10`
  - Max: `100`
- `pageNumber` (optional): Page number to retrieve. Must be greater than 0.
  - Type: `integer`
  - Default: `1`
- `sortBy` (optional): Field to sort by.
  - Options: `"title"`, `"genre"`, `"recommendedTimes"`
  - Default: `"title"`
- `sortOrder` (optional): Sort order.
  - Options: `"asc"`, `"desc"`
  - Default: `"asc"`

#### Example Request
```
GET /recommendations/history?pageSize=20&pageNumber=1&sortBy=recommendedTimes&sortOrder=desc
```

#### Response
```json
{
  "data": {
    "items": [
      {
        "title": "Valorant",
        "genre": "Shooter",
        "recommendedTimes": 15
      },
      {
        "title": "League of Legends",
        "genre": "Moba",
        "recommendedTimes": 12
      }
    ],
    "totalCount": 150,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

## 🔧 Configuration

### Environment Variables
- `MONGO_CONNECTION_STRING`: MongoDB connection string (default: `mongodb://localhost:27018`)
- `REDIS_CONNECTION_STRING`: Redis connection string (default: `localhost:6379`)
- `Proxies:FREE_GAMES_API_URL`: External games API URL (default: `https://www.freetogame.com/`)

## 🚀 Performance & Cache

### Caching Strategy
The application implements intelligent caching using Redis to optimize external API requests:

## 🧪 Testing

### Example cURL Commands

**Get a game recommendation:**
```bash
curl -X POST "http://localhost:7018/recommendation" \
  -H "Content-Type: application/json" \
  -d '{
    "genres": ["Shooter", "RPG"],
    "platform": "pc",
    "ramGb": 8
  }'
```

**Get paginated history:**
```bash
curl -X GET "http://localhost:7018/recommendations/history?pageSize=10&pageNumber=1&sortBy=recommendedTimes&sortOrder=desc"
```
