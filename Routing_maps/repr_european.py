#%%
import pandas as pd
import numpy as np
from sklearn.cluster import DBSCAN
import matplotlib.pyplot as plt
from pyproj import Proj, transform

df = pd.read_csv('E-Road_2011.csv', sep="\t", decimal=",")
# each row is a line: 
# xD & yD (for starting point) and xF & yF (for ending point) are the GPS coordinates

topX = df['xD'].max()
topY = df['yD'].max()
botX = df['xD'].min()
botY = df['yD'].min()

print(topX, topY, botX, botY)



# repr the map

# plt.figure(figsize=(10, 10))
# for i in range(len(df)):
#     plt.plot([df['xD'][i], df['xF'][i]], [df['yD'][i], df['yF'][i]], 'b')
#     plt.plot(df['xD'][i], df['yD'][i], 'ro')
#     plt.plot(df['xF'][i], df['yF'][i], 'ro')
# plt.show()

# clustering
coords = []
for i in range(len(df)):
    d = df['xD'][i], df['yD'][i]
    f = df['xF'][i], df['yF'][i]

    coords.append(d)
    coords.append(f)

coords = np.array(coords)
clustering = DBSCAN(eps=20_000, min_samples=1).fit(coords)



labels = clustering.labels_

unique_labels = set(labels)


# Plot the points
# for centroid in centroids:
#     plt.plot(centroid[0], centroid[1], 'go')
df_new = df.copy()
for label in unique_labels:
    cluster_points = coords[labels == label]
    centroid = cluster_points.mean(axis=0)
    # replace the points with the centroids
    for i in range(len(df_new)):
        if (df_new['xD'][i], df_new['yD'][i]) in cluster_points:
            df_new['xD'][i] = centroid[0]
            df_new['yD'][i] = centroid[1]
        if (df_new['xF'][i], df_new['yF'][i]) in cluster_points:
            df_new['xF'][i] = centroid[0]
            df_new['yF'][i] = centroid[1]

# remove the duplicates
df_new = df_new.drop_duplicates(subset=['xD', 'yD', 'xF', 'yF'])

#%%
nodes = []

plt.figure(figsize=(10, 10))
for i in df_new.index:
    color = {
        'N': 'g',
        'C': 'darkorange',
    }[df_new['concessionPrD'][i]]
    plt.plot([df_new['xD'][i], df_new['xF'][i]], [df_new['yD'][i], df_new['yF'][i]], color)
    nodes.append((df_new['xD'][i], df_new['yD'][i]))
    nodes.append((df_new['xF'][i], df_new['yF'][i]))


def latlon_to_lambert93(lon, lat):
    lambert93 = Proj(init='epsg:2154')
    wgs84 = Proj(init='epsg:4326')
    return transform(wgs84, lambert93, lon, lat)
def find_nearest_node(node, nodes):
    min_dist = 1e9
    nearest = None
    for n in nodes:
        dist = np.sqrt((n[0]-node[0])**2 + (n[1]-node[1])**2)
        if dist < min_dist:
            min_dist = dist
            nearest = n
    return nearest

cities = pd.read_csv('villes.csv')
new_lines = []
for i in cities.index:
    lat, lon = latlon_to_lambert93(cities['lattitude'][i], cities['longitude'][i])
    # check if in range:
    if topX > lat > botX and topY > lon > botY:
        nearest= find_nearest_node((lat, lon), nodes)
        new_lines.append([nearest[0], nearest[1], lat, lon])
        # nodes.append((lat, lon))
        plt.plot([nearest[0], lat], [nearest[1], lon], 'b')
        plt.plot(lat, lon, 'ro')
        
    

plt.show()

#%%
# export_roads = []

# for node in nodes:
#     export_nodes.append({
#         'x': node[0],
#         'y': node[1],
#         'pr
#     })