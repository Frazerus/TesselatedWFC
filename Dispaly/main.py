import seaborn as sb
import pandas as pd
import matplotlib.pyplot as plt


def data_normal(df):
    return df[df["Target_Method"] == "Normal Squares"]


def data_basic(df):
    return df[df["Target_Method"] == "Basic"]


def data_grass(df):
    return df[df["Target_Method"] == "Grass Squares"]

def chooseHeuristic(df, str):
    return df[df['heuristic'] == str]

def clean(dataFrame):
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

    dataFrame['s'] = dataFrame['Params'].str.rpartition('=')[2]

    dataFrame['size'] = 5
    dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('10'), '10')
    dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('20'), '20')
    dataFrame['size'] = dataFrame['size'].mask(dataFrame['Params'].str.contains('30'), '30')

    data30 = dataFrame[dataFrame["size"] == '30']
    data20 = dataFrame[dataFrame["size"] == '20']
    data10 = dataFrame[dataFrame["size"] == '10']

    dataNorm10 = data_normal(data10)
    dataNorm20 = data_normal(data20)
    dataNorm30 = data_normal(data30)

    dataBasic10 = data_basic(data10)
    dataBasic20 = data_basic(data20)
    dataBasic30 = data_basic(data30)

    dataGrass10 = data_grass(data10)
    dataGrass20 = data_grass(data20)
    dataGrass30 = data_grass(data30)

    dataNorm20 = dataNorm20[dataNorm20['Measurement_Value'] > 2000]
    dataNorm30 = dataNorm30[dataNorm30['Measurement_Value'] > 10_000]

    dataBasic20 = dataBasic20[dataBasic20['Measurement_Value'] > 200]
    dataBasic30 = dataBasic30[dataBasic30['Measurement_Value'] > 1200]

    dataGrass30 = dataGrass30[dataGrass30['Measurement_Value'] > 2500]
    dataGrass20 = dataGrass20[dataGrass20['Measurement_Value'] > 500]

    dataNorm = pd.concat([dataNorm10, dataNorm20, dataNorm30])
    dataBasic = pd.concat([dataBasic10, dataBasic20, dataBasic30])
    dataGrass = pd.concat([dataGrass10, dataGrass20, dataGrass30])

    df = pd.concat([dataBasic, dataGrass, dataNorm])
    df = df.drop(columns=df.columns[(df == 'Default').any()])

    df.to_csv('cleaned.csv', index=False)
    return df


df = pd.read_csv('cleaned.csv')

df10 = df[df["size"] == '10']
df20 = df[df["size"] == '20']
df30 = df[df["size"] == '30']

dataNorm10 = data_normal(df10)
dataNorm20 = data_normal(df20)
dataNorm30 = data_normal(df30)
dataBasic10 = data_basic(df10)
dataBasic20 = data_basic(df20)
dataBasic30 = data_basic(df30)
dataGrass10 = data_grass(df10)
dataGrass20 = data_grass(df20)
dataGrass30 = data_grass(df30)

entN10 = chooseHeuristic(dataNorm10, "entropy")
mrvN10 = chooseHeuristic(dataNorm10, "MRV")
entN20 = chooseHeuristic(dataNorm20, "entropy")
mrvN20 = chooseHeuristic(dataNorm20, "MRV")
entN30 = chooseHeuristic(dataNorm30, "entropy")
mrvN30 = chooseHeuristic(dataNorm30, "MRV")

entB10 = chooseHeuristic(dataBasic10, "entropy")
mrvB10 = chooseHeuristic(dataBasic10, "MRV")
entB20 = chooseHeuristic(dataBasic20, "entropy")
mrvB20 = chooseHeuristic(dataBasic20, "MRV")
entB30 = chooseHeuristic(dataBasic30, "entropy")
mrvB30 = chooseHeuristic(dataBasic30, "MRV")

entG10 = chooseHeuristic(dataGrass10, "entropy")
mrvG10 = chooseHeuristic(dataGrass10, "MRV")
entG20 = chooseHeuristic(dataGrass20, "entropy")
mrvG20 = chooseHeuristic(dataGrass20, "MRV")
entG30 = chooseHeuristic(dataGrass30, "entropy")
mrvG30 = chooseHeuristic(dataGrass30, "MRV")

entDescN10 = entN10.describe(exclude=['category'])
entDescN20 = entN20.describe(exclude=['category'])
entDescN30 = entN30.describe(exclude=['category'])

entDescB10 = entB10.describe(exclude=['category'])
entDescB20 = entB20.describe(exclude=['category'])
entDescB30 = entB30.describe(exclude=['category'])

entDescG10 = entG10.describe(exclude=['category'])
entDescG20 = entG20.describe(exclude=['category'])
entDescG30 = entG30.describe(exclude=['category'])


mrvDescN10 = mrvN10.describe(exclude=['category'])
mrvDescN20 = mrvN20.describe(exclude=['category'])
mrvDescN30 = mrvN30.describe(exclude=['category'])

mrvDescB10 = mrvB10.describe(exclude=['category'])
mrvDescB20 = mrvB20.describe(exclude=['category'])
mrvDescB30 = mrvB30.describe(exclude=['category'])

mrvDescG10 = mrvG10.describe(exclude=['category'])
mrvDescG20 = mrvG20.describe(exclude=['category'])
mrvDescG30 = mrvG30.describe(exclude=['category'])

descriptions=pd.concat([entDescN10, entDescN20, entDescN30, entDescB10, entDescB20, entDescB30, entDescG10, entDescG20, entDescG30, mrvDescN10, mrvDescN20, mrvDescN30, mrvDescB10, mrvDescB20,mrvDescB30, mrvDescG10, mrvDescG20, mrvDescG30])
descriptions.to_csv('descriptions.csv', index=True)

plt.figure(figsize=(10, 4))
sb.boxplot(data=df10, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 10")
plt.grid(axis='x')
plt.savefig('runtime10.pdf')

plt.figure(figsize=(10, 4))
sb.boxplot(data=df20, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 20")
plt.grid(axis='x')
plt.savefig('runtime20.pdf')

plt.figure(figsize=(10, 4))
sb.boxplot(data=df30, y="Target_Method", x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("")
plt.title("Runtime with size = 30")
plt.grid(axis='x')
plt.savefig('runtime30.pdf')

plt.figure(figsize=(10, 4))
sb.boxplot(data=data_normal(df), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Runtime of Octagon Tesselation Model with the normal squares tileset")
plt.grid(axis='x')
plt.savefig('runtimeNormal.pdf')

plt.figure(figsize=(10, 4))
sb.boxplot(data=data_basic(df), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in squares")
plt.title("Runtime of the original Model with the basic tileset")
plt.grid(axis='x')
plt.savefig('runtimeBasic.pdf')

plt.figure(figsize=(10, 4))
sb.boxplot(data=data_grass(df), y='size', x="Measurement_Value", gap=.1, hue="heuristic")
plt.xlabel('Time in µs')
plt.ylabel("Length and height of the grid in octagons")
plt.title("Runtime of Octagon Tesselation Model with the grass squares tileset")
plt.grid(axis='x')
plt.savefig('runtimeGrass.pdf')
plt.show()


###### Memory



plt.figure(figsize=(10, 4))
sb.barplot(data=df10, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("")
plt.title("Memory allocation with size = 10")
plt.grid(axis='x')
plt.savefig('memory10.pdf')

plt.figure(figsize=(10, 4))
sb.barplot(data=df20, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("")
plt.title("Memory allocation size = 20")
plt.grid(axis='x')
plt.savefig('memory20.pdf')

plt.figure(figsize=(10, 4))
sb.barplot(data=df30, y="Target_Method", x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("")
plt.title("Memory allocation size = 30")
plt.grid(axis='x')
plt.savefig('memory30.pdf')

plt.figure(figsize=(10, 4))
sb.barplot(data=data_normal(df), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("Length and height of the grid in octagons")
plt.title("Memory allocation of Octagon Tesselation Model with the normal squares tileset")
plt.grid(axis='x')
plt.savefig('memoryNormal.pdf')

plt.figure(figsize=(10, 4))
sb.barplot(data=data_basic(df), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("Length and height of the grid in squares")
plt.title("Memory allocation  of the original Model with the basic tileset")
plt.grid(axis='x')
plt.savefig('memoryBasic.pdf')

plt.figure(figsize=(10, 4))
sb.barplot(data=data_grass(df), y='size', x="AllocatedKB", gap=.1, hue="heuristic")
plt.xlabel('Allocated memory in KB')
plt.xlim(0,34000)
plt.ylabel("Length and height of the grid in octagons")
plt.title("Memory allocation  of Octagon Tesselation Model with the grass squares tileset")
plt.grid(axis='x')
plt.savefig('memoryGrass.pdf')
plt.show()