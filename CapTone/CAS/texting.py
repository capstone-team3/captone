# -*- coding: utf-8 -*-
import re
import sys
from flask import Flask,request,jsonify
from eunjeon import Mecab
from konlpy.tag import Kkma


app = Flask(__name__)
app.config['JSON_AS_ASCII'] = False
@app.route('/phone',methods=['POST'])
def dataret():
    jsonData = request.get_json()
    text = jsonData['text']

    seller_list = []
    order_list = []
    sel_cnt = 0
    real_seller='\0'
    breaky = 0
    seller = jsonData['seller']
    text = text.replace('\t', ' ').replace('^', '').replace('*', '').replace('/', ' ').replace(',', ' ').replace('.', ' ')
    text = text.replace('\r','')
    order = text.split('\n')
    for i in range(0, len(order)):
        for j in range(0, len(seller)):
            if seller[j] in order[i]:
                sel_cnt+=1
                real_seller=seller[j]
                break




    if(sel_cnt == 0):
        return jsonify({'seller':"seller_error",'itemlist':"ok",'name':"ok",'number':"ok",'address':"ok", 'items':"ok"})
    for i in range(0, len(order)):
        if real_seller not in order[i]:
            order_list.append(order[i])
        if real_seller in order[i]:
            seller_list.append(order[i])

    text = '\n'.join(order_list)
    pepsi = '\n'.join(seller_list)

    item = '\0'
    item_list = jsonData['thisisitem']

    unit = jsonData['unit'] #단위 사전 불러오기
    unit = unit.replace('\r','')
    unit_list = unit.split('\n')



    m = Mecab()

    if "카카오톡" in text:
        a = 1
    else:
        a = 2

    # 전처리 과정
    #kakao talk data
    if (a == 1):
        text = text[::-1]
        text = text[:text.find('---------------')]
        text = text[::-1]
        lastest = text

        text = text.replace('\n', ' ').replace('^', '').replace('*', '').replace('\t', ' ')
        text_list = text.split(' ')
        kakao_list = text_list
        prep = []  # []포함 사이 잘라내기

        for i in range(0, len(kakao_list)):
            prep.append(re.sub(r'\[[^)]*]', '', kakao_list[i]))

        hi = len(prep)
        for i in range(0, hi):
            if (i == len(prep)):
                break
            if '[' in prep[i]:
                del prep[i]
            if ']' in prep[i]:
                del prep[i]
        del prep[1]

        text = ' '.join(prep)

        for i in range(0, 200):
            if '' in prep:
                prep.remove('')
    #band data
    elif (a == 2):
        prep = []
        temp2 = []
        band_text = text
        band_text.replace('\t', ' ')
        band_sentence = band_text.split('\n')
        band_sentence = band_sentence[::-1]

        for i in range(0, len(band_sentence)):
            temp2.insert(i, band_sentence[i][:13])
        for i in range(0, len(band_sentence) - 2):
            if (temp2[i] != temp2[i + 1]):
                band_sentence = band_sentence[:i + 1]
                break

        band_sentence = band_sentence[::-1]

        lastest = '\n'.join(band_sentence)

        text = lastest

        for i in range(0, len(band_sentence)):
            temp = band_sentence[i].split(' ')
            del temp[:6]
            prep += temp

        for i in range(0, len(prep)):
            if '\n' in prep[i]:
                prep[i] = prep[i].replace('\n', '')

    print(prep)

    wang = 0
    for i in range(0, len(item_list)):
        if item_list[i] in text:
            wang += 1
    if(wang==0):
        return jsonify({'itemlist':"itemlist_error",'name':"ok",'number':"ok",'address':"ok", 'items':"ok", 'seller':"ok"})

    print(text)

    #전화번호 찾기
    phone = 0
    dash = ['-']
    phoneregex = re.compile(r'(\b)\d{2,3}(\s|''|-|.|~)(\d{3,4}(\s|''|-|.|~)\d{4})')

    if (None != re.search(phoneregex, lastest)):
        phone = re.search(phoneregex, lastest)

        phone = phone.group()  # 전화번호 추출
        phone = phone.replace('~', '').replace(' ', '').replace(',', '').replace('.', '').replace('-', '')
        phone = list(phone)

        if len(phone) == 11:
            phone = phone[:3] + dash + phone[3:7] + dash + phone[7:11]
        elif (len(phone) == 10) & (phone[1] == 2):
            phone = phone[:2] + dash + phone[2:6] + dash + phone[6:10]
        elif (len(phone) == 10) & (phone[1] != 2):
            phone = phone[:3] + dash + phone[3:6] + dash + phone[6:10]
        elif phone[:2] == "02":
            phone = phone[:2] + dash + phone[2:5] + dash + phone[5:9]
        else :
            phone = "phone_error"

        phone = ''.join(phone)
    else:
        phone = "phone_error"

    # 주소 찾기

    capital = jsonData['capital'] # 시도 불러오기
    capital = capital.replace('\r', '')
    capital_list = capital.split('\n')
    capital_list.remove('')

    dist = jsonData['dist']  # 시군구 불러오기
    dist = dist.replace('\n', ' ')
    dist = dist.replace('\r', '')
    dist_list = dist.split(' ')
    dist_list = list(set(dist_list))
    if '' in dist_list:
        dist_list.remove('')
    city_list = []
    county_list = []

    town = jsonData['town']  # 읍면동 불러오기
    town = town.replace('\r', '')
    town_list = town.split('\n')

    vill = jsonData['vill']  # 리 불러오기
    vill = vill.replace('\r', '')
    vill_list = vill.split('\n')
    vill_list = list(set(vill_list))
    vill_list.remove('')
    village_list = []

    road = jsonData['road'] #도로명 불러오기
    road = road.replace('\r','')
    road_list = road.split('\n')

    dregex = re.compile(r'\d{2,4}동')
    hregex = re.compile(r'\d{2,4}호')
    lnregex = re.compile(r'(\b)\d{1,4}-*\d{0,3}(\b)')
    building_regex = re.compile(r'(\b)\d{1,4}-*\d{0,1}(\b)')
    apartment = re.compile(r'\d{2,4}-\d{2,4}')

    add = ['','','','','','','','']
    addcount = 0

    adress_stc = ''
    adress_cnt=0
    lastest_sentence = lastest.split('\n')

    # 주소 문장 찾기
    for i in range(0, len(lastest_sentence)):
        for j in range(0, len(dist_list)):
            if dist_list[j] in lastest_sentence[i]:
                adress_stc = lastest_sentence[i]
                break

    if adress_stc == '':
        adress_cnt = 1

    for i in range(0, len(dist_list)): #시군구 파일 시 /군구 분리
        cltemp = list(dist_list[i])
        j = len(cltemp)
        if cltemp[j-1] == "시":
            city_list.append(dist_list[i]) #시
        else:
            county_list.append(dist_list[i]) #군구

    for i in range(0, len(vill_list)): #리 필터링
        vltemp = list(vill_list[i])
        j = len(vltemp)
        if vltemp[j-1] != "동":
            village_list.append(vill_list[i]) #리

    if (a == 1):
        adress_stc = re.sub(r'\[[^)]*]', '', adress_stc)

    elif (a == 2):
        adress_stc = adress_stc.split(' ')
        del adress_stc[:6]
        adress_stc = ' '.join(adress_stc)#adress_stc >> 주소 문장

    adress = m.morphs(adress_stc) #주소문장 형태소 분석
    adresssplt = adress_stc.split(' ') #주소문장 토큰화

    for i in range(0, 10):
        if '' in adresssplt:
            adresssplt.remove('')

    for i in range(0, len(adresssplt)):
        for j in range(0, len(road_list)):
            if adresssplt[i] == road_list[j]:
                road_type = 0#도로명
                breaky = 1
                break
            else:
                road_type = 1#지번
        if breaky == 1:
            breaky = 0
            break

    #도/광역시
    for i in range(0, len(adresssplt)):
        for j in range(0, len(capital_list)):
            if set(list(adresssplt[i])) & set(list(capital_list[j])) == set(list(adresssplt[i])):
                add[0] = capital_list[j]
                addidx = adress_stc.find(adresssplt[i])
                adress_stc = adress_stc[addidx:]
                adress_stc = adress_stc.replace(adresssplt[i], '',1)
                adress_stc = adress_stc.lstrip()
                adresssplt = adress_stc.split(' ')
                breaky = 1
                break
        if breaky ==1:
            breaky = 0
            break

    #시
    for i in range(0, len(adresssplt)):
        for j in range(0, len(city_list)):
            if adresssplt[i] == city_list[j]:
                add[1] = city_list[j]
                addidx = adress_stc.find(city_list[j])
                adress_stc = adress_stc[addidx:]
                adress_stc = adress_stc.replace(city_list[j], '',1)
                adress_stc = adress_stc.lstrip()
                adresssplt = adress_stc.split(' ')
                breaky = 1
                break
        if breaky == 1:
            breaky = 0
            break

    #군구
    for i in range(0, len(adress)):
        for j in range(0, len(county_list)):
            if adress[i] == county_list[j]:
                add[2] = county_list[j]
                addidx = adress_stc.find(county_list[j])
                adress_stc = adress_stc[addidx:]
                adress_stc = adress_stc.replace(county_list[j], '', 1)
                adress_stc = adress_stc.lstrip()
                adresssplt = adress_stc.split(' ')
                breaky = 1
                break
        if breaky == 1:
            breaky = 0
            break


    #읍면동
    for i in range(0, len(adresssplt)):
        for j in range(0, len(town_list)):
            if adresssplt[i] == town_list[j]:
                add[3] = town_list[j]
                addidx = adress_stc.find(town_list[j])
                adress_stc = adress_stc[addidx:]
                adress_stc = adress_stc.replace(town_list[j], '', 1)
                adress_stc = adress_stc.lstrip()
                adresssplt = adress_stc.split(' ')
                breaky = 1
                break
        if breaky == 1:
            breaky = 0
            break


    if road_type == 1:
        #리
        for i in range(0, len(adresssplt)):
            for j in range(0, len(village_list)):
                if adresssplt[i] == village_list[j]:
                    add[4] = village_list[j]
                    addidx = adress_stc.find(village_list[j])
                    adress_stc = adress_stc[addidx:]
                    adress_stc = adress_stc.replace(village_list[j], '', 1)
                    adress_stc = adress_stc.lstrip()
                    adresssplt = adress_stc.split(' ')
                    breaky = 1
                    break
            if breaky == 1:
                breaky = 0
                break

        add[5] = re.search(lnregex, adress_stc)#지번
        if add[5] == None:
            add[5] = ''
            print('지번이 입력되지 않아서 에러 리턴')
        else:
            add[5] = add[5].group()
            addidx = adress_stc.find(add[5])
            adress_stc = adress_stc[addidx:]
            adress_stc = adress_stc.replace(add[5], '', 1)
            adress_stc = adress_stc.lstrip()
            adresssplt = adress_stc.split(' ')


    elif road_type == 0:
        #도로명
        for i in range(0, len(adresssplt)):
            for j in range(0, len(road_list)):
                if adresssplt[i] == road_list[j]:
                    add[4] = road_list[j]
                    addidx = adress_stc.find(road_list[j])
                    adress_stc = adress_stc[addidx:]
                    adress_stc = adress_stc.replace(road_list[j], '', 1)
                    adress_stc = adress_stc.lstrip()
                    adresssplt = adress_stc.split(' ')
                    breaky = 1
                    break
            if breaky == 1:
               breaky = 0
               break

        add[5] = re.search(building_regex, adress_stc)#건물번호
        if add[5] == None:
            add[5] = ''
        else:
            add[5] = add[5].group()
            addidx = adress_stc.find(add[5])
            adress_stc = adress_stc[addidx:]
            adress_stc = adress_stc.replace(add[5], '', 1)
            adress_stc = adress_stc.lstrip()
            adresssplt = adress_stc.split(' ')

    if (re.search(apartment, adress_stc) != None):
        imtired = []
        imtired = re.search(apartment, adress_stc).group().split('-')
        add[6] = imtired[0] + '동'
        add[7] = imtired[1] + '호'
    else:
        add[6] = re.search(dregex, adress_stc)#동
        if add[6] == None:
            add[6] = ''
        else:
            add[6] = add[6].group()

        add[7] = re.search(hregex, adress_stc)#호
        if add[7] == None:
            add[7] = ''
        else:
            add[7] = add[7].group()

    #주소 일반화
    if (add[0] == ''):
        dic = open('C:/Users/korea/addressdic.txt', 'r', encoding='utf-8')
        dic = dic.read()  # 자동완성 사전 불러오기
        dic = dic.replace('\t', ' ')
        dic_list = dic.split('\n')
        dic_list = list(set(dic_list))
        dic_temp = []

        for i in range(0, len(dic_list)):
            if (add[2] in dic_list[i])&(add[3] in dic_list[i]):
                dic_temp = dic_list[i].split('\t')
                add[0] = dic_temp[0]

    #주소 합치기
    for i in range(0, 7):
        if '' in add:
            add.remove('')

    finad = ' '.join(add)



     #품목 행 찾기
    item_sentence = '\0'
    for i in range(0, len(lastest_sentence)):
        for j in range(0, len(item_list)):
            for k in range(0, len(unit_list)):
                if (item_list[j] in lastest_sentence[i]) & (unit_list[k] in lastest_sentence[i]):
                    item_sentence = lastest_sentence[i]
                    item = item_list[j]
                    unit = unit_list[k]
                    break
                #else: 에러 메세지 리턴

    #갯수 찾기
    sp = item_sentence.find(item)
    sp = sp + len(item)
    ep = item_sentence.find(unit)
    amount_pre = item_sentence[sp:ep]
    amount_pre = amount_pre.replace(' ', '').replace('\t', '')
    numregex = re.compile(r'(\b)[0-9]+(\b)')

    if (None != re.search(numregex, amount_pre)):
        ea = re.search(numregex, amount_pre)
        ea = ea.group()
    else:

        cardinal = jsonData['cardinal']
        cardinal = cardinal.split('\n')

        ea=0

        for i in range(0, len(cardinal)):
            if cardinal[i][:2].replace('-', '') in amount_pre:
                eatemp = re.findall("\d+", cardinal[i])
                eatemp = ''.join(eatemp)
                eatemp = int(eatemp)
                ea += eatemp
    ea=int(ea)
    one_amount = ['마리','미','개']
    twenty_amount = ['두름','축','태','쾌','코']
    #단위 일반화
    if unit in twenty_amount :
        ea *= 20
        unit = "EA"
    elif "톳" == unit:
        ea = ea * 100
        unit = "EA"
    elif "손" == unit:
        ea = ea * 2
        unit = "EA"
    elif (unit.count('g') + unit.count('m') + unit.count('k')) > 0:
        unit = unit.upper()
    elif unit in one_amount:
        unit = "EA"
    elif "로" in unit:
        unit = "KG"
    elif "리" in unit:
        unit = "MG"
    elif ("그" in unit)& len(list(unit)) == 2:
        unit = "G"

    ea = str(ea)

    #품목 문장 합치기
    item = item + ' ' + ea + unit

    if(adress_cnt == 1):
        finad = "address_error"



    # 이름 찾기
    name = jsonData['firstname']
    name = name.replace('\r','')
    name = name.split('\n')

    family_name = jsonData['lastname']
    family_name = family_name.replace('\r','')
    family_name = family_name.split('\n')
    family_long = []

    for i in range(0, len(family_name)):
        if len(family_name[i]) == 2:
            family_long.append(family_name[i])

    name_cdd = []
    name_result = []
    name_tmp1 = []
    name_tmp2 = []
    name_tmp3 = []
    name_err = []

    for i in range(0, len(name)):
        if name[i] in text:
            name_cdd.append(name[i])

    print(name_cdd)

    for i in range(0, len(name_cdd)):

        fam_idx = text.find(name_cdd[i])

        if text[fam_idx - 1] == ' ':
            fam_str = text[fam_idx - 3:fam_idx - 1]
            spacecount = 1
        else:
            fam_str = text[fam_idx - 2:fam_idx]
            spacecount = 0

        fam_str = fam_str.replace(' ', '')

        if len(fam_str) == 2:
            for j in range(0, len(family_long)):
                if fam_str == family_long[j]:
                    plused = (family_long[j] + name_cdd[i])
                    name_result.append(plused)

        elif len(fam_str) == 1:
            for j in range(0, len(family_name)):
                if fam_str == family_name[j]:
                    plused = (family_name[j] + name_cdd[i])
                    name_result.append(plused)

    print(name_result)
    print(pepsi)
    for i in range(0, len(name_result)):
        if name_result[i] not in pepsi:
            name_tmp1.append(name_result[i])
    print(name_tmp1)
    for i in range(0, len(name_tmp1)):
        if name_tmp1[i] not in item:
            name_tmp2.append(name_tmp1[i])
    print(name_tmp2)
    for i in range(0, len(name_tmp2)):
        if name_tmp2[i] not in finad:
            name_tmp3.append(name_tmp2[i])
    print(name_tmp3)
    for i in range(0, len(name_tmp3)):
        for j in range(0, len(name_tmp3)):
            if (name_tmp3[i] in name_tmp3[j]) & (len(name_tmp3[i]) < len(name_tmp3[j])):
                name_err.append(name_tmp3[i])
    print(name_err)
    name_result = list(set(name_tmp3) - set(name_err))

    if len(name_result) == 1:
        finname = ''.join(name_result)
    else:
        finname = "name_error"

    print(finname)
    print(phone)
    print(finad)
    print(item)


    return jsonify({'name':finname,'number':phone,'address':finad, 'items':item,'seller':"OK",'itemlist':"OK"})

if __name__ == '__main__':
    import os
    HOST = os.environ.get('SERVER_HOST', 'localhost')

    app.run(HOST, port=5000)