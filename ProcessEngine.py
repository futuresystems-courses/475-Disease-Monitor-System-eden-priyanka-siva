__author__ = 'spalanivel'
import sys
from pymongo import MongoClient
import time;
from datetime import datetime
import json
import urllib.request

# Connect to database
try:
    connection = MongoClient('localhost',27017)
except Exception:
        sys.stderr.write("Could not connect to MongoDB: %s" % e)
        sys.exit(1)
db = connection.DiseaseMonitor
print ('Started to search Keyword');
dt = datetime.now()
print ('Start time :', dt)
count1 = db.tweets.find({'$and':[{'$text':{'$search':'Breast'}},{'HashTag':'cancer'}]}).count()
count2 = db.tweets.find({'$and':[{'$text':{'$search':'Kidney'}},{'HashTag':'cancer'}]}).count()
count3 = db.tweets.find({'$and':[{'$text':{'$search':'Skin'}},{'HashTag':'cancer'}]}).count()
count4 = db.tweets.find({'$and':[{'$text':{'$search':'Lung'}},{'HashTag':'cancer'}]}).count()
count5 = db.tweets.find({'$and':[{'$text':{'$search':'Liver'}},{'HashTag':'cancer'}]}).count()
count6 = db.tweets.find({'$and':[{'$text':{'$search':'Thyroid'}},{'HashTag':'cancer'}]}).count()
count7 = db.tweets.find({'$and':[{'$text':{'$search':'Brain'}},{'HashTag':'cancer'}]}).count()
db.CancerCount.delete_many({})
db.CancerCount.insert({'Type':'Breast','Cnt':count1})
db.CancerCount.insert({'Type':'Kidney','Cnt':count2})
db.CancerCount.insert({'Type':'Skin','Cnt':count3})
db.CancerCount.insert({'Type':'Lung','Cnt':count4})
db.CancerCount.insert({'Type':'Liver','Cnt':count5})
db.CancerCount.insert({'Type':'Thyroid','Cnt':count6})
db.CancerCount.insert({'Type':'Brain','Cnt':count7})

print('Total count '+str(count1))
print('Total count '+str(count2))
print('Total count '+str(count3))
print('Total count '+str(count4))
print('Total count '+str(count5))
print('Total count '+str(count6))
print('Total count '+str(count7))

cnt1 = db.tweets.find({'$and':[{'$text':{'$search':'headache'}},{'HashTag':'flu'}]}).count()
cnt2 = db.tweets.find({'$and':[{'$text':{'$search':'cough'}},{'HashTag':'flu'}]}).count()
cnt3 = db.tweets.find({'$and':[{'$text':{'$search':'sore'}},{'HashTag':'flu'}]}).count()
cnt4 = db.tweets.find({'$and':[{'$text':{'$search':'nose'}},{'HashTag':'flu'}]}).count()
cnt5 = db.tweets.find({'$and':[{'$text':{'$search':'pain'}},{'HashTag':'flu'}]}).count()
cnt6 = db.tweets.find({'$and':[{'$text':{'$search':'dizziness'}},{'HashTag':'flu'}]}).count()
cnt7 = db.tweets.find({'$and':[{'$text':{'$search':'vomiting'}},{'HashTag':'flu'}]}).count()

db.FluSymptoms.delete_many({})
db.FluSymptoms.insert({'Type':'Headache','Cnt':cnt1})
db.FluSymptoms.insert({'Type':'Cough','Cnt':cnt2})
db.FluSymptoms.insert({'Type':'Sore throat','Cnt':cnt3})
db.FluSymptoms.insert({'Type':'Running nose','Cnt':cnt4})
db.FluSymptoms.insert({'Type':'Muscle pain','Cnt':cnt5})
db.FluSymptoms.insert({'Type':'Dizziness','Cnt':cnt6})
db.FluSymptoms.insert({'Type':'Vomiting','Cnt':cnt7})


print('Total count '+str(cnt1))
print('Total count '+str(cnt2))
print('Total count '+str(cnt3))
print('Total count '+str(cnt4))
print('Total count '+str(cnt5))
print('Total count '+str(cnt6))
print('Total count '+str(cnt7))

print('Running google api to fetch location country for latitude and longitude. Please wait...')
del1 = db.FluLocationCount.update_one({},{'$set':{'Cnt':0}})
del2 = db.CancerLocationCount.update_one({},{'$set':{'Cnt':0}})

for doc in db.tweets.find({'X':{'$gt':'0'},'HashTag' : 'flu'}):
    url = "http://maps.googleapis.com/maps/api/geocode/json?"
    url += "latlng=%s,%s&sensor=false" % (doc["X"], doc["Y"])
    response = urllib.request.urlopen(url)
    str_response = response.readall().decode('utf-8')
    obj = json.loads(str_response)
    cc = 0
    for result in obj['results']:
        for component in result['address_components']:
            if 'country' in component['types']:
                cc=cc+1
                country = component['long_name']
                if cc == 1:                    
                            result_FL = db.FluLocationCount.update_one({'Type':country }, {'$inc':{'Cnt':1}},upsert=True)
                            print(result_FL.matched_count)
                            print(country)
                            
print('Running google api to fetch location country for latitude and longitude. Please wait...')
for doc in db.tweets.find({'X':{'$gt':'0'},'HashTag' : 'cancer'}):
    urlcancer = "http://maps.googleapis.com/maps/api/geocode/json?"
    urlcancer += "latlng=%s,%s&sensor=false" % (doc["X"], doc["Y"])
    response = urllib.request.urlopen(urlcancer)
    str_response = response.readall().decode('utf-8')
    obj = json.loads(str_response)
    cc = 0
    for result in obj['results']:
        for component in result['address_components']:
            if 'country' in component['types']:
                cc=cc+1
                country = component['long_name']
                if cc == 1:                    
                            result_Cancer = db.CancerLocationCount.update_one({'Type':country }, {'$inc':{'Cnt':1}},upsert=True)
                            print(result_Cancer.matched_count)
                            print(country)
                   
#db.LocationCount.copyTo('FluLocationCount')
#db.FluLocationCount.update({},{$set:{"Cnt":0}},{multi:true})

dt = datetime.now()
print ('End time :', dt)
print()

