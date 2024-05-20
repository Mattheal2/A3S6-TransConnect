namespace TransLib.Itinerary;

public class RouteNode {
    #region Nested Classes
    /// <summary>
    /// Represents a city in the route.
    /// </summary>
    public class NodeCity {
        /// <summary>
        /// The name of the city.
        /// </summary>
        public string name;
        /// <summary>
        /// The postal code of the city.
        /// </summary>
        public int postal_code;

        public NodeCity(string name, int postal_code) {
            this.name = name;
            this.postal_code = postal_code;
        }
    }

    public class NodeLink {
        /// <summary>
        /// The id of the node that this link points to.
        /// </summary>
        public int id;
        /// <summary>
        /// The distance between the two nodes, in meters.
        /// </summary>
        public int distance;
        /// <summary>
        /// The kind of link, one of "road", "highway", "national".
        /// </summary>
        public string kind;

        public NodeLink(int id, int distance, string kind) {
            this.id = id;
            this.distance = distance;
            this.kind = kind;
        }

        /// <summary>
        /// Returns the cost of the link in euro-cents (only highways have a cost, the rest are free).
        /// </summary>
        /// <returns></returns>
        public int GetRoadCost() {
            switch (kind) {
                case "highway":
                    // https://www.ouest-france.fr/economie/transports/autoroute/peages-sur-lautoroute-qui-fixent-leurs-prix-et-sur-quels-criteres-on-vous-repond-5f68e75c-19bd-11ee-a274-cd245df77ae9
                    // => ~ 13 cents per kilometer
                    return (int)(distance / 1000.0f * 13);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns the time it takes to travel the link in seconds.
        /// </summary>
        /// <returns></returns>
        public int GetRoadTime() {
            // distance en m

            int speed = 80; // km/h
            switch (kind) {
                case "highway":
                    speed = 130;
                    break;
                default:
                    break;
            }

            float speed_m_s = speed / 3.6f;

            return (int)(distance / speed_m_s);
        }
    }

    #endregion

    #region Properties
    public int id;
    public NodeCity? city;
    public int[] coords;
    public NodeLink[] links;
    #endregion

    #region Constructors
    public RouteNode(int id, NodeCity? city, int[] coords, NodeLink[] links) {
        this.id = id;
        this.city = city;
        this.coords = coords;
        this.links = links;
    }
    #endregion
}