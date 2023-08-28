import matplotlib.pyplot as plt,keyboard,time,requests


def get_data_from_api(api_route,headers=None):
    try:
        response = requests.get(api_route,headers=headers)
        response.raise_for_status() 
        # Raise an exception for 4xx and 5xx status codes!
        data = response.json()
        return data
    except requests.exceptions.RequestException as e:
        print("Error fetching data:", e)
        return None


api_route = input("Enter the API route: ")

headers = {
    # Example authorization header
    'x-api-key': '7b2dd5db-27db-46b6-b6c3-18f2a547b243',
}

count = 1
x_values = []
y_values = []

while True:
    launch = get_data_from_api(api_route+'/g1/ngforce/ignition',headers=headers)
    if (keyboard.is_pressed('L') or keyboard.is_pressed('l')):
        get_data_from_api(api_route+'/g1/ngforce/ignition/1',headers=headers)
    if launch:
        data = get_data_from_api(api_route+'/g1/ngforce/engine/last',headers=headers)
        if data is not None:
            x_values.append(count)
            y_values.append(data)
            count += 1
            print("Thrust: ", data)
        else:
            print("No entry has been made!")
    if (keyboard.is_pressed('Q') or keyboard.is_pressed('q')):
        plt.plot(x_values,y_values)
        plt.show()
        print("// Terminated!")
        break
    if (keyboard.is_pressed('P') or keyboard.is_pressed('p')):
        last100 = get_data_from_api(api_route+'/g1/ngforce/engine',headers=headers)
        if(len(last100)):
            plt.plot(range(1,len(last100)+1),last100)
            plt.show()
