import seaborn as sb
import pandas as pd
import matplotlib.pyplot as plt


def dataNormal(df):
    return df[df["Target_Method"] == "Normal Squares"]


def dataBasic(df):
    return df[df["Target_Method"] == "Basic"]


def dataGrass(df):
    return df[df["Target_Method"] == "Grass Squares"]


dataFrame = pd.read_csv('measurements.csv')
dataFrame.replace("OriginalModel", "Basic", inplace=True)
dataFrame.replace("OctagonTessellationModel_NormalSquares", "Normal Squares", inplace=True)
dataFrame.replace("OctagonTessellationModel_GrassSquares", "Grass Squares", inplace=True)

dataFrame = dataFrame[dataFrame["Measurement_IterationMode"] == "Workload"]
dataFrame = dataFrame[dataFrame["Measurement_IterationStage"] == "Result"]

dataFrame['Measurement_Value'] = dataFrame['Measurement_Value']/1000
dataFrame['AllocatedKB'] = dataFrame['Allocated_Bytes']/1024

dataFrame['heuristic'] = 5
dataFrame['heuristic'] = dataFrame['heuristic'].mask(dataFrame['Params'].str.contains('Entropy'), 'entropy')
dataFrame['heuristic'] = dataFrame['heuristic'].mask(dataFrame['Params'].str.contains('MRV'), 'MRV')

dataFrame['size'] = 5
dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('10'), '10')
dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('20'), '20')
dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('30'), '30')

data30 = dataFrame[dataFrame["size"] == '30']
data20 = dataFrame[dataFrame["size"] == '20']
data10 = dataFrame[dataFrame["size"] == '10']

plt.figure(figsize=(15, 6))
sb.boxplot(data=data10, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 10")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.boxplot(data=data20, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 20")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.boxplot(data=data30, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 30")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.boxplot(data=dataNormal(dataFrame), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Runtime of Octagon Tesselation Model with the normal squares tileset")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.boxplot(data=dataBasic(dataFrame), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in squares")
plt.title("Runtime of the original Model with the basic tileset")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.boxplot(data=dataGrass(dataFrame), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Runtime of Octagon Tesselation Model with the grass squares tileset")
plt.grid(axis='x')
plt.show()


###### Memory



plt.figure(figsize=(15, 6))
sb.barplot(data=data10, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("")
plt.title("Memory allocation with size = 10")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.barplot(data=data20, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("")
plt.title("Memory allocation size = 20")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.barplot(data=data30, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("")
plt.title("Memory allocation size = 30")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.barplot(data=dataNormal(dataFrame), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Memory allocation of Octagon Tesselation Model with the normal squares tileset")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.barplot(data=dataBasic(dataFrame), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("Length and height of the grid in squares")
plt.title("Memory allocation  of the original Model with the basic tileset")
plt.grid(axis='x')

plt.figure(figsize=(15, 6))
sb.barplot(data=dataGrass(dataFrame), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Memory allocation  of Octagon Tesselation Model with the grass squares tileset")
plt.grid(axis='x')
plt.show()