using System.Text.Json.Serialization;
using System.Text.Json;

namespace TransLib.Itinerary;

public class ItineraryService {
    private RouteNode[] nodes;

    private ItineraryService(RouteNode[] nodes) {
        this.nodes = nodes;
    }

    /// <summary>
    /// Load all nodes from Routing_maps/nodes.json.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static ItineraryService Load(string path) {
        string json = File.ReadAllText(path);
        RouteNode[]? nodes = JsonSerializer.Deserialize<RouteNode[]>(json, new JsonSerializerOptions { IncludeFields = true }); ;
        if (nodes == null) throw new Exception("Failed to load nodes.");
        return new ItineraryService(nodes);
    }
    
    public RouteNode GetNode(int id) {
        return nodes[id];
    }

    public RouteNode[] GetNodes() {
        return nodes;
    }
    
    

    /// <summary>
    /// Find the shortest route between two nodes.
    /// </summary>
    /// <param name="start">The starting node.</param>
    /// <param name="end">The ending node.</param>
    /// <param name="heuristic">The heuristic function to use.</param>
    /// <param name="cost">The cost function to use.</param>
    /// <returns></returns>
    /// <exception cref="Exception">If no route is found.</exception>
    public Itinerary GetRoute(RouteNode start, RouteNode end, Heuristic heuristic, Cost cost) {
        Dictionary<int, int> cameFrom = new();
        Dictionary<int, int> gScore = new();
        Dictionary<int, int> fScore = new();
        HashSet<int> openSet = new();
        HashSet<int> closedSet = new();

        gScore[start.id] = 0;
        fScore[start.id] = heuristic(start, end);
        openSet.Add(start.id);

        while (openSet.Count > 0) {
            RouteNode current = nodes[openSet.OrderBy(node => fScore[node]).First()];

            // Route found
            if (current == end) return ReconstructPath(cameFrom, current);

            openSet.Remove(current.id);
            closedSet.Add(current.id);
            foreach (RouteNode.NodeLink link in current.links) {
                RouteNode neighbor = nodes[link.id];
                if (closedSet.Contains(neighbor.id)) continue;
                int tentativeGScore = gScore[current.id] + cost(link);
                if (!openSet.Contains(neighbor.id) || tentativeGScore < gScore[neighbor.id]) {
                    cameFrom[neighbor.id] = current.id;
                    gScore[neighbor.id] = tentativeGScore;
                    fScore[neighbor.id] = gScore[neighbor.id] + heuristic(neighbor, end);
                    if (!openSet.Contains(neighbor.id)) openSet.Add(neighbor.id);
                }
            }
        }
        throw new Exception("No route found.");
    }

    /// <summary>
    /// Reconstruct the path from the cameFrom dictionary.
    /// </summary>
    /// <param name="cameFrom"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private Itinerary ReconstructPath(Dictionary<int, int> cameFrom, RouteNode current) {
        List<RouteNode> path = new();
        path.Append(current);
        while (cameFrom.ContainsKey(current.id)) {
            current = nodes[cameFrom[current.id]];
            path.Append(current);
        }
        
        int[] reversed = new int[path.Length];
        for (int i = 0; i < path.Length; i++) {
            reversed[i] = path.Get(path.Length - i - 1).id;
        }

        // Calculate the distance, cost and time of the route
        int distance = 0;
        int cost = 0;
        int time = 0;

        for (int i = 0; i < reversed.Length - 1; i++) {
            RouteNode.NodeLink link = nodes[reversed[i]].links.First(link => link.id == reversed[i + 1]);
            distance += link.distance;
            cost += link.GetRoadCost();
            time += link.GetRoadTime();
        }
        return new Itinerary(reversed, distance, cost, time);
    }
    
    #region Heuristics
    /// <summary>
    /// Heuristic function for the A* algorithm.
    /// This is usually the distance between the two nodes.
    /// </summary>
    /// <param name="start">Initial node.</param>
    /// <param name="end">End node.</param>
    /// <returns>Estimated score.</returns>
    public delegate int Heuristic(RouteNode start, RouteNode end);
    /// <summary>
    /// Manhattan distance heuristic.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static int ManhattanDistance(RouteNode start, RouteNode end) {
        return Math.Abs(start.coords[0] - end.coords[0]) + Math.Abs(start.coords[1] - end.coords[1]);
    }

    /// <summary>
    /// Euclidean distance heuristic.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static int EuclideanDistance(RouteNode start, RouteNode end) {
        return (int)Math.Sqrt(Math.Pow(start.coords[0] - end.coords[0], 2) + Math.Pow(start.coords[1] - end.coords[1], 2));
    }
    #endregion

    #region Costs
    /// <summary>
    /// Cost function for the A* algorithm.
    /// Unlike the heuristic, this is the actual cost of the route, between two directly connected nodes.
    /// </summary>
    /// <param name="link">The link to calculate the cost for.</param>
    /// <returns></returns>
    public delegate int Cost(RouteNode.NodeLink link);

    /// <summary>
    /// Cost function based on the distance between the two nodes.
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    public static int DistanceCost(RouteNode.NodeLink link) {
        return link.distance;
    }

    /// <summary>
    /// Cost function based on the time it takes to travel between the two nodes.
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    public static int TimeCost(RouteNode.NodeLink link) {
        return link.GetRoadTime();
    }

    /// <summary>
    /// Cost function based on the cost of the road (only highways have a cost).
    /// </summary>
    /// <param name="link"></param>
    /// <returns></returns>
    public static int RoadCost(RouteNode.NodeLink link) {
        return link.GetRoadCost();
    }
    #endregion

}