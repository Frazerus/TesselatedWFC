import seaborn as sb
import pandas as pd
import matplotlib.pyplot as plt

dataFrame = pd.read_csv('measurements.csv')
dataFrame = dataFrame[dataFrame["Measurement_IterationMode"] == "Workload"]
dataFrame = dataFrame[dataFrame["Measurement_IterationStage"] == "Result"]
dataFrame['Measurement_Value'] = dataFrame['Measurement_Value']/1000
dataFrame.replace("OriginalModel_MRV", "Basic MRV", inplace=True)
dataFrame.replace("OriginalModel_Entropy", "Basic Entropy", inplace=True)
dataFrame.replace("OctagonTessellationModel_NormalSquares_MRV", "Normal Squares MRV", inplace=True)
dataFrame.replace("OctagonTessellationModel_NormalSquares_Entropy", "Normal Squares Entropy", inplace=True)
dataFrame.replace("OctagonTessellationModel_GrassSquares_MRV", "Grass Squares MRV", inplace=True)
dataFrame.replace("OctagonTessellationModel_GrassSquares_Entropy", "Grass Squares Entropy", inplace=True)

dataFrame = dataFrame[dataFrame["Target_Method"] != "Grass Squares MRV"]
dataFrame = dataFrame[dataFrame["Target_Method"] != "Grass Squares Entropy"]
dataFrame = dataFrame[dataFrame["Target_Method"] != "Normal Squares MRV"]
dataFrame = dataFrame[dataFrame["Target_Method"] != "Normal Squares Entropy"]

plt.figure(figsize=(15, 6))
plot = sb.boxplot(data=dataFrame, y="Target_Method", x="Measurement_Value", gap=.1,)
plt.xlabel('Time in µs')
plt.ylabel("")
plt.xlim(50, 250)
plt.title("Runtime")

plt.grid(axis='x')
plt.show()