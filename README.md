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
  - Options: `["Shooter", "RPG", "Strategy", "MMO", "Racing", "Sports", "Social", "Sandbox", "Open World", "Survival", "Pvp", "Pve", "Pixel Graphics", "Voxel Graphics", "Zombie", "First Person", "Third Person", "Top-Down", "3D Graphics", "2D Graphics", "Anime", "Fantasy", "Action", "Fighting", "Battle Royale", "Military", "Martial Arts", "Flight", "Low Spec", "Tank", "Space", "Sailing", "Side Scroller", "Superhero", "Permadeath", "Card", "Battle-Card", "Card Game", "Auto Battler", "Action Roguelike", "Moba", "City Builder", "Racing", "Sports", "Social", "Sandbox", "Open World", "Survival", "Pvp", "Pve", "Pixel Graphics", "Voxel Graphics", "Zombie", "First Person", "Third Person", "Top-Down", "3D Graphics", "2D Graphics", "Anime", "Fantasy", "Action", "Fighting", "Battle Royale", "Military", "Martial Arts", "Flight", "Low Spec", "Tank", "Space", "Sailing", "Side Scroller", "Superhero", "Permadeath", "Card", "Battle-Card", "Card Game", "Auto Battler", "Action Roguelike", "Moba", "City Builder"]`
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

## 🏗️ Architecture

The application follows Clean Architecture principles

## 🔧 Configuration

### Environment Variables
- `MONGO_CONNECTION_STRING`: MongoDB connection string (default: `mongodb://localhost:27018`)
- `REDIS_CONNECTION_STRING`: Redis connection string (default: `localhost:6379`)
- `Proxies:FREE_GAMES_API_URL`: External games API URL (default: `https://www.freetogame.com/`)

### Docker Configuration
The `docker-compose.yaml` file sets up:
- MongoDB instance on port 27018
- Redis instance on port 6379
- Database name: `gamepicker`
- Collection: `GameRecommendations`

## 🚀 Performance & Cache

### Caching Strategy
The application implements intelligent caching using Redis to optimize performance:

#### **1. Game Recommendations Cache**
- **Duration**: 5 minutes
- **Key**: `recommendation_{genres}_{platform}_{ramGb}`
- **Purpose**: Avoids recalculating recommendations for identical requests

#### **2. Filtered Games Cache**
- **Duration**: 1 hour
- **Key**: `filtered_games_{genres}_{platform}`
- **Purpose**: Reduces external API calls for the same filter combinations

#### **3. Game Details Cache**
- **Duration**: 24 hours
- **Key**: `game_details_{gameId}`
- **Purpose**: Caches individual game information (rarely changes)

#### **4. History Cache**
- **Duration**: 3 days
- **Key**: `history_{pageSize}_{pageNumber}_{sortBy}_{sortOrder}`
- **Purpose**: Speeds up paginated history requests

#### **Cache Invalidation**
- History cache is automatically invalidated when new recommendations are saved
- Cache keys are designed to be unique per request parameters
- Fallback to database/external API if cache is unavailable

## 🛠️ Development

### Project Structure
```
GamePickerChallenge/
├── GamePicker/                 # API Layer
├── GamePicker.Application/     # Application Layer
├── GamePicker.Domain/          # Domain Layer
├── GamePicker.Infastructure/   # Infrastructure Layer
├── GamePicker.Contracts/       # DTO Layer
└── docker-compose.yaml         # Docker configuration
```

### Key Features
- **Standardized API Responses**: All responses follow a consistent format with `data`, `message`, `errors`, and `statusCode` fields
- **Error Handling**: Comprehensive error handling with custom exceptions and middleware
- **Pagination**: Database-level pagination for efficient data retrieval
- **Sorting**: Flexible sorting options for history endpoint
- **External API Integration**: Integration with FreeToGame API for game data
- **MongoDB**: NoSQL database for storing game recommendations
- **Swagger Documentation**: Auto-generated API documentation

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
