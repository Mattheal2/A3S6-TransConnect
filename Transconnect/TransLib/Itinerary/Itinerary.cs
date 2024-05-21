namespace TransLib.Itinerary;

public class Itinerary {
    public int[] nodes;

    /// <summary>
    /// The distance between the two nodes, in meters.
    /// </summary>
    public int distance { get; }
    /// <summary>
    /// The cost of the route, in euro-cents.
    /// </summary>
    public int cost { get; }
    /// <summary>
    /// The time it takes to travel the route, in seconds.
    /// </summary>
    public int time { get; }

    public Itinerary(int[] nodes, int distance, int cost, int time) {
        this.nodes = nodes;
        this.distance = distance;
        this.cost = cost;
        this.time = time;
    }
}